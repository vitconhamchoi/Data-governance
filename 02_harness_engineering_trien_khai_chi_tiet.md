# Bài 21: Harness Engineering, Lộ Trình Triển Khai Chi Tiết

## Mục tiêu bài học

- Hiểu harness engineering như một bài toán sản phẩm và hệ thống, không chỉ là prompt engineering
- Biết cách chia nhỏ kiến trúc thành các lớp dễ triển khai, dễ đo lường, dễ mở rộng
- Có một lộ trình build thực tế phù hợp với profile fullstack .NET, Angular, PostgreSQL, queue, realtime
- Có checklist triển khai từ POC đến production
- Biết nơi nào nên deterministic bằng code, nơi nào nên giao cho LLM quyết định

---

## 21.1 Harness engineering là gì

Harness engineering là thiết kế và triển khai cái khung điều phối để AI làm việc ổn định trong môi trường thật. 

Thay vì chỉ hỏi "prompt sao cho hay", harness engineering hỏi:

- Input nào được chấp nhận
- Task được phân loại ra sao
- Khi nào gọi tool, khi nào hỏi lại người dùng
- Dữ liệu nào cần nhớ, nhớ bao lâu, lưu ở đâu
- Lỗi nào tự retry, lỗi nào phải escalation
- Hành động nào cần approval
- Chất lượng đầu ra đo bằng gì
- Làm sao replay một run để debug
- Làm sao thay model mà không phá contract

Nói ngắn gọn, đây là phần giao nhau giữa:

- AI application architecture
- distributed systems
- platform engineering
- observability
- policy and safety
- product workflow design

---

## 21.2 Tư duy đúng trước khi bắt tay build

### Sai lầm phổ biến

- Bắt đầu bằng prompt trước khi có state model
- Nhét quá nhiều quyền vào agent quá sớm
- Để LLM quyết định cả những phần deterministic
- Không log tool call, không replay được session
- Không có dataset để eval, chỉ test bằng cảm giác
- Trộn lẫn UI logic, orchestration logic, tool adapter, và persistence

### Nguyên tắc triển khai

1. Code cứng phần contract, policy, timeout, retry, permission
2. Để LLM quyết phần reasoning, decomposition, summarization, synthesis
3. Mọi tool đều phải có schema input/output rõ ràng
4. Mọi run quan trọng đều phải trace được
5. Muốn scale thì thiết kế event trước, UI sau
6. POC có thể sync, production nên async hóa dần

---

## 21.3 Kiến trúc tham chiếu cho sếp

Phù hợp với stack:

- Backend: ASP.NET Core
- Frontend: Angular
- Database: PostgreSQL
- Queue/Event: Redis Streams, RabbitMQ, hoặc Kafka
- Realtime: SignalR
- Worker: .NET BackgroundService
- Storage: S3 compatible hoặc local object storage

### 5 lớp chính

1. **Experience layer**
   - Telegram, Web chat, Admin UI, CLI

2. **Orchestration layer**
   - Session coordinator
   - Task router
   - Planner
   - Approval manager
   - Execution state machine

3. **Tool harness layer**
   - Web search adapter
   - File adapter
   - Git adapter
   - Shell adapter
   - Database query adapter
   - Internal API adapter

4. **State and memory layer**
   - Conversation history
   - Task state
   - Long-term memory
   - Vector index nếu cần semantic recall
   - Audit log

5. **Evaluation and governance layer**
   - Run tracing
   - Cost tracking
   - Latency tracking
   - Success/failure rubric
   - Approval boundary
   - Replay and test harness

---

## 21.4 Kiến trúc deployment tối thiểu

```text
Angular Admin / Chat UI
        |
        v
ASP.NET Core API Gateway
        |
        +--> Session Service
        +--> Orchestrator Service
        +--> Tool Registry
        +--> Approval Service
        +--> SignalR Hub
        |
        +--> PostgreSQL
        +--> Redis
        +--> Queue Broker
        +--> Worker Service
        +--> LLM Provider Adapter
```

### Trách nhiệm từng thành phần

#### API Gateway

- Nhận message từ UI hoặc external channel
- Authenticate user và tenant
- Chuẩn hóa request về internal command
- Đẩy job vào orchestration flow

#### Session Service

- Tạo session
- Lưu message history
- Gắn user, workspace, permissions, memory scope
- Trả context ngắn gọn cho orchestrator

#### Orchestrator Service

- Phân loại task
- Chọn model
- Chọn strategy, direct answer, tool-use, plan-and-execute, multi-agent
- Tạo execution graph hoặc step list
- Quản lý retries, escalation, cancelation

#### Tool Registry

- Công bố metadata của tool
- Validate input bằng schema
- Chuẩn hóa output và error
- Gắn timeout, retry policy, side effect level

#### Worker Service

- Chạy task dài
- Chạy subtask song song nếu cần
- Stream tiến độ về UI qua SignalR
- Persist intermediate results

#### Approval Service

- Chặn hành động nhạy cảm
- Tạo approval request
- Chờ user approve hoặc deny
- Resume flow khi có quyết định

---

## 21.5 Data model nên có

### Bảng `sessions`

- `id`
- `user_id`
- `channel`
- `title`
- `status`
- `created_at`
- `last_activity_at`

### Bảng `messages`

- `id`
- `session_id`
- `role`
- `content`
- `content_json`
- `created_at`

### Bảng `runs`

- `id`
- `session_id`
- `task_type`
- `strategy`
- `model`
- `status`
- `started_at`
- `completed_at`
- `latency_ms`
- `prompt_tokens`
- `completion_tokens`
- `cost_usd`

### Bảng `run_steps`

- `id`
- `run_id`
- `step_no`
- `step_type`
- `tool_name`
- `input_json`
- `output_json`
- `status`
- `started_at`
- `completed_at`
- `error_code`

### Bảng `approvals`

- `id`
- `run_id`
- `action_type`
- `requested_payload`
- `status`
- `requested_at`
- `resolved_at`
- `resolved_by`

### Bảng `memories`

- `id`
- `scope`
- `scope_id`
- `memory_type`
- `content`
- `embedding`
- `importance_score`
- `created_at`

### Bảng `tool_definitions`

- `name`
- `version`
- `input_schema`
- `output_schema`
- `side_effect_level`
- `timeout_ms`
- `retry_policy`

---

## 21.6 State machine cho một run

Một run không nên chỉ là một hàm gọi model. Nên model hóa rõ trạng thái.

### Trạng thái đề xuất

- `pending`
- `classifying`
- `planning`
- `awaiting_approval`
- `executing`
- `waiting_external`
- `synthesizing`
- `completed`
- `failed`
- `cancelled`

### Luồng cơ bản

1. User gửi message
2. Session service chuẩn hóa context
3. Orchestrator classify task
4. Router chọn strategy
5. Nếu cần kế hoạch, planner sinh steps
6. Nếu có step nhạy cảm, chuyển `awaiting_approval`
7. Worker hoặc direct executor chạy step
8. Tool result được persist vào `run_steps`
9. Orchestrator tổng hợp kết quả
10. Trả final answer và đóng run

### Vì sao quan trọng

Khi có state machine, sếp sẽ có:

- khả năng resume flow
- retry từng bước thay vì retry cả run
- metrics theo từng trạng thái
- UI hiển thị tiến độ tốt hơn
- debug rõ nguyên nhân fail

---

## 21.7 Task routing strategy

Không phải task nào cũng cần cùng một loại agent.

### 4 chiến lược tối thiểu

#### 1. Direct answer
Dùng cho:
- hỏi đáp ngắn
- giải thích khái niệm
- formatting, summarization nhẹ

Ưu điểm:
- nhanh
- rẻ
- ít lỗi điều phối

#### 2. Tool-assisted answer
Dùng cho:
- cần web search
- cần đọc file
- cần query database
- cần dữ liệu mới

Ưu điểm:
- chính xác hơn nhớ máy
- giảm hallucination

#### 3. Plan-and-execute
Dùng cho:
- task nhiều bước
- coding, refactor, migration, audit
- task có phụ thuộc giữa các bước

Ưu điểm:
- kiểm soát tốt hơn
- trace tốt hơn

#### 4. Multi-agent
Dùng cho:
- research phức tạp
- review chéo nhiều góc nhìn
- pipeline dài có chuyên môn hóa

Nhược điểm:
- tốn tiền
- tốn latency
- tăng complexity

Khuyến nghị thực tế: chỉ dùng multi-agent sau khi single-agent harness đã ổn.

---

## 21.8 Tool contract, phần sống còn của harness

Mỗi tool cần tối thiểu:

- `name`
- `description`
- `input_schema`
- `output_schema`
- `timeout_ms`
- `retry_policy`
- `idempotency`
- `side_effect_level`
- `required_permissions`

### Phân loại side effect

- `read_only`
- `workspace_write`
- `external_write`
- `destructive`

### Ví dụ hợp đồng tool

```json
{
  "name": "web_search",
  "side_effect_level": "read_only",
  "timeout_ms": 10000,
  "retry_policy": {
    "max_attempts": 2,
    "backoff_ms": 1000
  },
  "required_permissions": ["web:read"]
}
```

### Rule quan trọng

- Tool lỗi phải trả error code chuẩn hóa
- Tool không bao giờ ném raw exception thẳng lên model
- Output phải đủ gọn để feed lại cho model
- Tool write phải log payload trước và sau khi thực thi

---

## 21.9 Approval boundary

Đây là chỗ nhiều hệ AI làm rất ẩu.

### Những action nên require approval

- gửi tin nhắn ra ngoài
- push git
- gọi API ghi dữ liệu production
- chạy shell có side effect
- xóa file hoặc sửa nhiều file
- giao dịch tài chính

### Thiết kế flow approval

1. Agent chuẩn bị action proposal
2. Hệ thống render proposal thành dạng người dùng hiểu được
3. User approve hoặc deny
4. Hệ thống lưu audit trail
5. Run resume từ checkpoint trước đó

### Nguyên tắc UX

Approval card nên cho thấy:

- agent muốn làm gì
- lên hệ nào
- payload nào sẽ chạy
- rủi ro chính là gì
- đây là allow once hay allow always

---

## 21.10 Memory design

Đừng xây memory chung chung. Nên tách rõ.

### 1. Conversation memory

- lịch sử hội thoại gần nhất
- giữ ngắn và liên quan
- dùng cho coherence

### 2. Working memory

- scratchpad theo task
- intermediate outputs
- planning state
- selected sources

### 3. Long-term memory

- preference người dùng
- facts bền hơn theo user/project
- lessons learned
- reusable playbooks

### 4. Retrieval memory

- vector search khi cần semantic recall
- không phải cái gì cũng cần embedding

### Rule thực dụng

- Chỉ promote vào long-term memory khi thật sự có giá trị lâu dài
- Memory ghi bằng code hoặc heuristic trước, đừng phó mặc hoàn toàn cho LLM
- Có expiration hoặc review policy cho memory dễ cũ

---

## 21.11 Observability, chỗ quyết định hệ này có dùng được không

Nếu sếp không quan sát được hệ chạy thế nào thì sớm muộn cũng bỏ cuộc.

### Metrics tối thiểu

- số run mỗi ngày
- tỉ lệ thành công
- p50, p95 latency
- token usage theo model
- cost theo tenant, user, feature
- tỉ lệ approval/deny
- tỉ lệ tool error
- số run cần escalation sang người

### Logs nên có

- input normalized
- selected strategy
- selected model
- tool call input/output đã sanitize
- state transitions
- final answer
- error chain

### Tracing nên có

Một trace cần nối được:

- user message
- run
- step
- tool call
- external request
- final response

Nếu có OpenTelemetry thì càng tốt. Rất hợp với .NET.

---

## 21.12 Evaluation framework

Harness engineering mà không có eval thì chỉ là demo đẹp.

### 3 loại eval nên build

#### 1. Regression eval
- 50 đến 200 prompt/case cố định
- test lại sau mỗi thay đổi prompt, tool, router

#### 2. Task success eval
- ví dụ: research task có tìm đúng nguồn không
- coding task có sửa đúng file không
- ops task có tạo approval đúng không

#### 3. Safety eval
- có tự gửi external write khi chưa approve không
- có lộ dữ liệu không nên lộ không
- có gọi sai tool permission không

### Rubric chấm nên rõ

Ví dụ cho research task:
- factuality
- citation quality
- completeness
- actionability
- brevity

### Rule quan trọng

- Lưu goldens
- So sánh trước/sau khi thay đổi harness
- Có vài eval deterministic, vài eval model-graded

---

## 21.13 Lộ trình triển khai 90 ngày

## Giai đoạn 1, ngày 1 đến 14, dựng skeleton

### Mục tiêu

Có một harness tối thiểu nhưng chuẩn nền.

### Việc cần làm

1. Tạo solution gồm:
   - `Harness.Api`
   - `Harness.Application`
   - `Harness.Domain`
   - `Harness.Infrastructure`
   - `Harness.Worker`
   - `Harness.Contracts`

2. Dựng PostgreSQL schema cơ bản:
   - sessions
   - messages
   - runs
   - run_steps
   - approvals

3. Dựng Redis cho:
   - cache
   - distributed lock
   - pub/sub hoặc stream nhẹ

4. Dựng 3 tool đầu tiên:
   - web search
   - file read
   - file write với approval

5. Dựng 3 execution strategy:
   - direct answer
   - tool-assisted
   - plan-and-execute đơn giản

6. Thêm SignalR để stream trạng thái run

### Deliverable

- Gửi message vào UI được
- Tạo run được
- Gọi ít nhất 2 tool được
- Log từng step vào database được

---

## Giai đoạn 2, ngày 15 đến 30, làm orchestration cho ra dáng

### Mục tiêu

Có router, planner, state machine, approval flow.

### Việc cần làm

1. Viết task classifier
2. Viết strategy router
3. Implement state machine rõ ràng
4. Thêm approval workflow cho action nhạy cảm
5. Chuẩn hóa error model:
   - validation_error
   - permission_denied
   - timeout
   - dependency_failed
   - tool_unavailable

6. Tạo admin page Angular để xem:
   - sessions
   - runs
   - step timeline
   - approval requests

### Deliverable

- Task multi-step chạy ổn định
- Approval card hoạt động
- UI xem được timeline một run

---

## Giai đoạn 3, ngày 31 đến 50, thêm memory và reliability

### Mục tiêu

Hệ đỡ ngớ ngẩn hơn và recover tốt hơn.

### Việc cần làm

1. Tách memory thành:
   - conversation memory
   - working memory
   - long-term memory

2. Thêm retry policy theo tool
3. Thêm circuit breaker cho provider và external API
4. Thêm checkpoint/resume cho task dài
5. Thêm queue consumer cho background execution
6. Thêm rate limit theo user/tenant

### Deliverable

- Run dài không block request thread
- Có resume khi worker restart
- Context được quản lý gọn hơn

---

## Giai đoạn 4, ngày 51 đến 70, eval và observability

### Mục tiêu

Bắt đầu biến demo thành hệ có thể tối ưu.

### Việc cần làm

1. Dựng OpenTelemetry tracing
2. Đẩy metrics sang Prometheus hoặc dashboard tương đương
3. Tạo bộ eval nội bộ khoảng 50 case
4. Thêm replay tool cho một run bất kỳ
5. Tracking cost theo model, user, tenant

### Deliverable

- Biết thay đổi nào làm hệ tốt hơn hay tệ đi
- Debug được run lỗi mà không đoán mò

---

## Giai đoạn 5, ngày 71 đến 90, hardening production

### Mục tiêu

Chuẩn bị cho môi trường nhiều user và nhiều channel.

### Việc cần làm

1. Multi-tenant permission matrix
2. Secret management đúng chuẩn
3. Quota theo team hoặc customer
4. Audit log immutable cho external write
5. Sandbox cho shell/file actions
6. Background janitor job:
   - cleanup expired run artifacts
   - archive old traces
   - compact memory

### Deliverable

- Có thể mời user thật vào thử
- Có baseline vận hành production

---

## 21.14 Tổ chức code trong .NET

### Domain

Chứa:
- Session aggregate
- Run aggregate
- Approval aggregate
- ToolDefinition
- domain events
- policies

### Application

Chứa:
- use cases
- orchestrator interfaces
- strategy selection
- command handlers
- DTOs

### Infrastructure

Chứa:
- EF Core repositories
- provider adapters
- queue adapters
- search adapters
- file system adapters
- observability integrations

### Presentation

Chứa:
- REST API
- SignalR hubs
- webhook endpoints
- auth filters

### Worker

Chứa:
- long-running executor
- queue consumers
- scheduler jobs
- retry processor

---

## 21.15 Angular admin UI nên có gì

### Màn Session List
- session id
- user
- channel
- last activity
- active run

### Màn Run Detail
- input
- strategy
- model
- step timeline
- tool results
- token usage
- cost
- final answer

### Màn Approval Queue
- pending approvals
- payload preview
- approve/deny
- resolved history

### Màn Eval Dashboard
- recent regressions
- failed cases
- latency trend
- cost trend
- top failing tools

---

## 21.16 3 project portfolio nên làm nếu muốn giỏi mảng này

### Project 1, AI coding harness

Mục tiêu:
- đọc codebase
- đề xuất plan
- sửa file có approval
- commit code
- sinh review summary

Chứng minh được:
- tool harness
- file safety
- git integration
- plan-and-execute

### Project 2, AI research and briefing harness

Mục tiêu:
- research web nhiều nguồn
- trích nguồn
- tổng hợp brief theo template
- lưu memory theo domain

Chứng minh được:
- retrieval
- summarization quality
- factuality eval
- source handling

### Project 3, AI ops assistant

Mục tiêu:
- đọc logs
- chạy health checks
- đề xuất remediation
- approval trước khi tác động hệ thống

Chứng minh được:
- observability integration
- policy boundary
- reliability engineering mindset

---

## 21.17 Sai lầm cần tránh nếu muốn đi xa

- Biến harness thành một đống if-else không có state model
- Tool nào cũng cho model gọi trực tiếp
- Không version schema của tool
- Không lưu cost và latency
- Không có approval cho external write
- Không có replay cho run lỗi
- Dùng vector DB cho mọi thứ dù không cần
- Xài multi-agent quá sớm chỉ vì thấy ngầu

---

## 21.18 Kế hoạch học tập cá nhân cho sếp

Nếu sếp muốn dùng nó như roadmap nghề nghiệp, em đề xuất:

### Tháng 1
- Ôn distributed systems fundamentals
- Build single-agent harness đơn giản
- Làm tracing + tool schema + approval cơ bản

### Tháng 2
- Build planner/router
- Build memory model tử tế
- Add background worker + queue

### Tháng 3
- Build eval suite
- Hardening observability
- Viết 2 case study thật kỹ lên GitHub

### Sau đó
- Đem đi apply remote job hoặc consulting
- Chuyển từ “prompt guy” sang “AI systems engineer”

---

## 21.19 Kết luận

Harness engineering là game của:

- systems thinking
- product thinking
- reliability
- safety boundary
- observability
- evaluation discipline

Ai chỉ giỏi prompt thì làm demo nhanh.
Ai giỏi harness mới build được hệ AI dùng được lâu.

Với nền .NET, Angular, PostgreSQL, SignalR, event-driven của sếp, em nghĩ đây là hướng rất hợp. Không cần nhảy sang thứ quá lạ, chỉ cần đóng gói lại năng lực hiện có quanh bài toán AI orchestration.

---

## 21.20 Checklist triển khai nhanh

### POC Checklist
- [ ] 1 UI nhận chat
- [ ] 1 orchestrator service
- [ ] 3 tools có schema
- [ ] 1 database lưu runs và steps
- [ ] 1 approval flow
- [ ] 1 dashboard xem traces

### Beta Checklist
- [ ] queue + worker
- [ ] retry + timeout + circuit breaker
- [ ] memory separation
- [ ] eval suite
- [ ] cost tracking
- [ ] replay run

### Production Checklist
- [ ] multi-tenant authz
- [ ] audit log
- [ ] secret management
- [ ] tool permission matrix
- [ ] SLO dashboard
- [ ] on-call runbook

---

## Bài tập thực hành

1. Thiết kế schema PostgreSQL cho `runs`, `run_steps`, `approvals`
2. Viết `IToolExecutor` và `IToolRegistry` trong .NET
3. Viết state machine cho 5 trạng thái đầu tiên của run
4. Dựng Angular page hiển thị step timeline
5. Tạo 20 test case eval cho một research harness

Nếu làm xong 5 bài tập này tử tế, sếp đã đi được một đoạn khá xa trong harness engineering rồi.