# 02b — Harness Engineering: Technical Design Specification

## 1. Mục đích tài liệu

Tài liệu này định nghĩa data model, state machine, sequence flow, tool contract, và error catalog đủ để implement và review kiến trúc.
Đây là nguồn tham chiếu kỹ thuật — không phải tài liệu giải thích khái niệm.

---

## 2. Data Model / ERD

```mermaid
erDiagram
    sessions {
        uuid id PK
        uuid user_id
        varchar channel
        varchar title
        varchar status
        timestamp created_at
        timestamp last_activity_at
    }

    messages {
        uuid id PK
        uuid session_id FK
        varchar role
        text content
        jsonb content_json
        timestamp created_at
    }

    runs {
        uuid id PK
        uuid session_id FK
        varchar task_type
        varchar strategy
        varchar model
        varchar status
        timestamp started_at
        timestamp completed_at
        int latency_ms
        int prompt_tokens
        int completion_tokens
        numeric cost_usd
    }

    run_steps {
        uuid id PK
        uuid run_id FK
        int step_no
        varchar step_type
        varchar tool_name
        jsonb input_json
        jsonb output_json
        varchar status
        timestamp started_at
        timestamp completed_at
        varchar error_code
    }

    approvals {
        uuid id PK
        uuid run_id FK
        varchar action_type
        jsonb requested_payload
        varchar status
        timestamp requested_at
        timestamp resolved_at
        uuid resolved_by
    }

    memories {
        uuid id PK
        varchar scope
        uuid scope_id
        varchar memory_type
        text content
        vector embedding
        numeric importance_score
        timestamp created_at
    }

    tool_definitions {
        varchar name PK
        varchar version
        jsonb input_schema
        jsonb output_schema
        varchar side_effect_level
        int timeout_ms
        jsonb retry_policy
    }

    sessions ||--o{ messages : "has"
    sessions ||--o{ runs : "has"
    runs ||--o{ run_steps : "has"
    runs ||--o{ approvals : "has"
    run_steps }o--|| tool_definitions : "uses"
```

---

## 3. Run State Machine

```mermaid
stateDiagram-v2
    [*] --> pending : run created

    pending --> classifying : start processing
    classifying --> planning : complex task
    classifying --> executing : simple / direct answer

    planning --> awaiting_approval : sensitive step detected
    planning --> executing : no approval needed

    awaiting_approval --> executing : user approved
    awaiting_approval --> cancelled : user denied

    executing --> waiting_external : external call in progress
    waiting_external --> executing : external response received
    waiting_external --> failed : external timeout

    executing --> synthesizing : all steps done
    synthesizing --> completed : final answer ready

    executing --> failed : unrecoverable error
    classifying --> failed : classification error
    planning --> failed : planning error

    pending --> cancelled : user cancelled
    executing --> cancelled : user cancelled mid-run

    completed --> [*]
    failed --> [*]
    cancelled --> [*]
```

---

## 4. Sequence Diagram — Luồng cơ bản

```mermaid
sequenceDiagram
    actor User
    participant GW as API Gateway
    participant SS as Session Service
    participant ORC as Orchestrator
    participant TR as Tool Registry
    participant W as Worker
    participant SIG as SignalR Hub

    User->>GW: POST /api/chat {message}
    GW->>SS: CreateOrGetSession(userId, channel)
    SS-->>GW: sessionId, context
    GW->>ORC: DispatchRun(sessionId, message)
    ORC-->>GW: runId (202 Accepted)
    GW-->>User: {runId}

    ORC->>ORC: ClassifyTask(message)
    ORC->>ORC: SelectStrategy(taskType)
    ORC->>TR: GetToolManifest(toolName)
    TR-->>ORC: toolSchema, policy

    ORC->>W: EnqueueStep(runId, stepPayload)
    W->>TR: ExecuteTool(toolName, input)
    TR-->>W: toolOutput
    W->>SS: PersistRunStep(runId, stepResult)
    W->>SIG: StreamProgress(runId, status)
    SIG-->>User: realtime update

    W->>ORC: StepCompleted(runId, stepNo)
    ORC->>ORC: Synthesize(allStepOutputs)
    ORC->>SS: SaveFinalAnswer(runId, answer)
    ORC->>SIG: StreamCompleted(runId, answer)
    SIG-->>User: final answer
```

---

## 5. Tool Contract Specification

### Các field bắt buộc

| Field | Type | Mô tả |
|---|---|---|
| `name` | string | Tên duy nhất của tool, snake_case |
| `description` | string | Mô tả ngắn cho LLM hiểu khi nào dùng tool này |
| `input_schema` | JSON Schema | Schema validate input trước khi gọi tool |
| `output_schema` | JSON Schema | Schema mô tả cấu trúc output trả về |
| `timeout_ms` | int | Timeout tối đa cho một lần gọi |
| `retry_policy` | object | `max_attempts`, `backoff_ms`, `retry_on` |
| `idempotency` | bool | `true` nếu gọi lại với cùng input cho cùng kết quả |
| `side_effect_level` | enum | `read_only` / `workspace_write` / `external_write` / `destructive` |
| `required_permissions` | string[] | Danh sách permission scope cần có |

### Ví dụ JSON — `web_search`

```json
{
  "name": "web_search",
  "description": "Search the public web and return top results with title, url, and snippet.",
  "input_schema": {
    "type": "object",
    "properties": {
      "query": { "type": "string", "maxLength": 256 },
      "max_results": { "type": "integer", "default": 5, "maximum": 10 }
    },
    "required": ["query"]
  },
  "output_schema": {
    "type": "array",
    "items": {
      "type": "object",
      "properties": {
        "title": { "type": "string" },
        "url": { "type": "string", "format": "uri" },
        "snippet": { "type": "string" }
      }
    }
  },
  "timeout_ms": 10000,
  "retry_policy": {
    "max_attempts": 2,
    "backoff_ms": 1000,
    "retry_on": ["timeout", "rate_limit"]
  },
  "idempotency": true,
  "side_effect_level": "read_only",
  "required_permissions": ["web:read"]
}
```

---

## 6. Tool Side Effect Classification

| Level | Mô tả | Ví dụ tool |
|---|---|---|
| `read_only` | Chỉ đọc, không thay đổi bất kỳ trạng thái nào | `web_search`, `file_read`, `db_query_select` |
| `workspace_write` | Ghi vào workspace nội bộ (file local, DB nội bộ) | `file_write`, `create_draft`, `save_note` |
| `external_write` | Gọi API bên ngoài có ghi dữ liệu | `send_email`, `post_slack`, `github_push`, `api_post` |
| `destructive` | Xóa hoặc thay đổi không thể hoàn tác | `delete_file`, `drop_table`, `cancel_payment`, `shell_rm` |

---

## 7. Task Routing Strategy

| Strategy | Dùng khi | Ưu điểm | Nhược điểm |
|---|---|---|---|
| **Direct Answer** | Hỏi đáp ngắn, giải thích khái niệm, format nhẹ | Nhanh, rẻ, ít lỗi điều phối | Không có tool call, không dùng được dữ liệu mới |
| **Tool-Assisted Answer** | Cần web search, đọc file, query DB, dữ liệu realtime | Chính xác hơn, giảm hallucination | Chậm hơn direct, cần tool schema chuẩn |
| **Plan-and-Execute** | Task nhiều bước, coding, refactor, migration, audit | Kiểm soát tốt, trace tốt, retry từng bước | Latency cao hơn, tốn token hơn |
| **Multi-Agent** | Research phức tạp, review chéo, pipeline chuyên môn hóa | Xử lý song song, chuyên sâu từng agent | Tốn tiền, tăng latency, tăng complexity — chỉ dùng khi single-agent đã ổn |

---

## 8. Error Code Catalog

| error_code | Mô tả | Hành động xử lý |
|---|---|---|
| `validation_error` | Input không đúng schema | Trả lỗi ngay cho client, không retry |
| `permission_denied` | User hoặc tenant thiếu permission | Trả 403, log audit, không retry |
| `tool_timeout` | Tool vượt `timeout_ms` | Retry theo `retry_policy`, sau đó fail step |
| `tool_unavailable` | Tool service không phản hồi | Circuit breaker, fallback hoặc fail run |
| `dependency_failed` | Bước phụ thuộc trước đó failed | Fail cả run hoặc skip tùy config |
| `rate_limit` | LLM provider hoặc tool trả 429 | Exponential backoff, retry |
| `context_overflow` | Prompt vượt context window của model | Truncate hoặc summarize context rồi retry |
| `approval_timeout` | Approval request không được giải quyết trong TTL | Tự động cancel run, notify user |
| `approval_denied` | User deny action | Cancel run step, log audit trail |
| `external_api_error` | External API trả lỗi 5xx | Retry nếu idempotent, fail nếu không |
| `memory_write_error` | Không ghi được vào memory store | Log warning, tiếp tục run không lưu memory |
| `model_error` | LLM trả lỗi hoặc output không parse được | Retry một lần với prompt đơn giản hơn |

---

## 9. Approval Flow Sequence

```mermaid
sequenceDiagram
    actor User
    participant ORC as Orchestrator
    participant APR as Approval Service
    participant DB as Database
    participant SIG as SignalR Hub

    ORC->>ORC: Detect sensitive action (side_effect_level != read_only)
    ORC->>APR: CreateApprovalRequest(runId, actionType, payload)
    APR->>DB: INSERT approvals (status=pending)
    APR-->>ORC: approvalId
    ORC->>ORC: Pause run (status=awaiting_approval)

    APR->>SIG: NotifyPendingApproval(userId, approvalId, payload)
    SIG-->>User: Show approval card (what · why · risk · allow_once/always)

    alt User approves
        User->>APR: POST /approvals/{id}/approve
        APR->>DB: UPDATE approvals (status=approved)
        APR->>ORC: ResumeRun(runId)
        ORC->>ORC: Continue from checkpoint
    else User denies
        User->>APR: POST /approvals/{id}/deny
        APR->>DB: UPDATE approvals (status=denied)
        APR->>ORC: CancelRun(runId)
        ORC->>SIG: NotifyRunCancelled(runId)
        SIG-->>User: Run cancelled
    else Timeout
        APR->>APR: TTL expired
        APR->>DB: UPDATE approvals (status=timeout)
        APR->>ORC: CancelRun(runId)
    end
```

---

## 10. Memory Type Comparison

| Type | Scope | TTL | Dùng khi | Lưu ở đâu |
|---|---|---|---|---|
| **Conversation Memory** | Session | Hết session | Giữ coherence trong hội thoại ngắn | `messages` table (PostgreSQL) |
| **Working Memory** | Run | Hết run | Scratchpad, intermediate output, planning state | `run_steps` + Redis (ephemeral) |
| **Long-term Memory** | User / Project | Vĩnh cửu hoặc review định kỳ | Preference, facts bền, lessons learned, playbook | `memories` table (PostgreSQL) |
| **Retrieval Memory** | Global / Tenant | Quản lý bằng policy | Semantic recall từ knowledge base lớn | pgvector / Qdrant |
