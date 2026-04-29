# 02a — Harness Engineering: Architecture Overview

## 1. Mục đích tài liệu

Tài liệu này giới thiệu kiến trúc tổng thể của Harness Engineering cho người mới tiếp cận hệ thống.
Mục tiêu: hiểu harness nằm ở đâu trong hệ thống lớn hơn, gồm những lớp nào, và dùng công nghệ gì.

---

## 2. Harness Engineering là gì

- Là khung điều phối (orchestration framework) để AI làm việc ổn định trong môi trường production
- Không chỉ là prompt engineering — là thiết kế hệ thống đầy đủ với state, policy, tool, memory, và eval
- Trả lời các câu hỏi: input nào hợp lệ, task được phân loại ra sao, tool nào được gọi, lỗi nào tự retry, hành động nào cần approval, chất lượng đầu ra được đo thế nào
- Giao điểm giữa: AI application architecture, distributed systems, platform engineering, observability, policy & safety, product workflow design

---

## 3. System Context Diagram

> **Scope:** Level-1 context map (C4 Model — Context View).  
> Mục đích: xác định **ai** tương tác với hệ thống, qua **kênh** nào, và hệ thống phụ thuộc vào **external system** nào.  
> Chi tiết internal component xem Section 4.

```mermaid
graph TB
    %% ── External Actors ───────────────────────────────────────────────
    subgraph ACTORS["  External Actors"]
        direction LR
        HU["👤 Human User\n─────────────\nEnd-user / Operator"]
        SA["🤖 System Agent\n─────────────\nScheduler · CI/CD · Webhook"]
        ADM2["🛡️ Platform Admin\n─────────────\nOps / SRE Team"]
    end

    %% ── Harness Platform (System Boundary) ───────────────────────────
    subgraph SYS["⬛  Harness Engineering Platform  [ System Boundary ]"]
        direction TB

        subgraph EDGE["  Edge & Auth Zone"]
            direction LR
            GW2["🔀 API Gateway\n+ Auth / Rate-limit"]
            RT2["📡 Realtime Hub\n(SignalR / WS)"]
        end

        subgraph CORE["  Orchestration Core"]
            direction LR
            ORC2["🧠 Orchestrator\n(Plan · Dispatch · Gate)"]
            TOOL["🔧 Tool Harness\n(Registry · Executor)"]
            MEM2["💾 State & Memory\n(Session · History · Vector)"]
        end

        subgraph OBS["  Observability & Governance"]
            direction LR
            TRACE["🔭 Tracing\n(OTEL · Jaeger)"]
            EVAL2["🧪 Eval Suite\n(Quality · Cost · Latency)"]
            AUDIT["📋 Audit Log\n(Immutable · Policy Gate)"]
        end
    end

    %% ── External Systems ──────────────────────────────────────────────
    subgraph EXT["  External Systems & Providers"]
        direction LR
        LLM2["🤖 LLM Providers\nOpenAI · Azure OAI · Gemini"]
        DS["🗄️ Enterprise Data\nPostgreSQL · S3 · Internal APIs"]
        NOTIFY["📣 Notification Channel\nTelegram · Email · PagerDuty"]
        IDPROV["🔐 Identity Provider\nOIDC / Azure AD / Auth0"]
    end

    %% ── Actor → Platform ──────────────────────────────────────────────
    HU    -->|"HTTPS / WebSocket\n(chat · task submit)"| EDGE
    SA    -->|"REST API · Webhook\n(automated trigger)"| EDGE
    ADM2  -->|"Admin API · Dashboard\n(config · monitor)"| EDGE

    %% ── Edge → Core ───────────────────────────────────────────────────
    EDGE  -->|"Authenticated command\n+ session token"| CORE
    CORE  -->|"Realtime progress event\n(SSE / WS push)"| RT2
    RT2   -->|"Live update stream"| HU

    %% ── Core → Observability ──────────────────────────────────────────
    CORE  -->|"Span · metric · decision log"| OBS

    %% ── Platform → External Systems ──────────────────────────────────
    CORE  -->|"LLM API call\n(prompt · function spec)"| LLM2
    CORE  -->|"Read / Write\n(structured + vector)"| DS
    OBS   -->|"Alert · incident"| NOTIFY
    EDGE  -->|"Token introspection\n(JWKS · userinfo)"| IDPROV

    %% ── Styling ───────────────────────────────────────────────────────
    classDef actorStyle  fill:#f0f9ff,stroke:#0ea5e9,color:#0c4a6e,font-weight:bold
    classDef edgeStyle   fill:#ede9fe,stroke:#7c3aed,color:#2e1065
    classDef coreStyle   fill:#dcfce7,stroke:#16a34a,color:#14532d
    classDef obsStyle    fill:#fef9c3,stroke:#ca8a04,color:#713f12
    classDef extStyle    fill:#f1f5f9,stroke:#64748b,color:#1e293b,stroke-dasharray:5 4

    class HU,SA,ADM2 actorStyle
    class GW2,RT2 edgeStyle
    class ORC2,TOOL,MEM2 coreStyle
    class TRACE,EVAL2,AUDIT obsStyle
    class LLM2,DS,NOTIFY,IDPROV extStyle
```

### Giải thích các zone

| Zone | Vai trò | Ranh giới bảo mật |
|---|---|---|
| **Edge & Auth** | Điểm vào duy nhất — xác thực, rate-limit, route | Public-facing, TLS terminated |
| **Orchestration Core** | Não của hệ thống — plan, dispatch, tool call, state | Internal network, không expose trực tiếp |
| **Observability & Governance** | Ghi lại mọi quyết định, đo chất lượng, policy gate | Read-only từ Core; write đến external alerting |
| **External Systems** | LLM, database, notification, identity — nằm ngoài system boundary | Giao tiếp qua adapter, không hardcode credential |

---

## 4. Component Diagram

```mermaid
graph TB
    %% ── Experience Layer ──────────────────────────────────────────
    subgraph EXP["🖥️  Experience Layer"]
        direction LR
        WEB["🌐 Web Chat UI"]
        TG["✈️  Telegram Bot"]
        ADM["⚙️  Admin UI\n(Angular)"]
        CLI["💻 CLI Client"]
    end

    %% ── Orchestration Layer ───────────────────────────────────────
    subgraph ORC["🧠  Orchestration Layer"]
        direction LR
        SC["📋 Session\nCoordinator"]
        TR2["🔀 Task\nRouter"]
        PL["📐 Planner"]
        SM["⚙️  State\nMachine"]
        APR2["✅ Approval\nManager"]
    end

    %% ── Tool Harness Layer ────────────────────────────────────────
    subgraph TH["🔧  Tool Harness Layer"]
        direction LR
        WS["🔍 Web Search\nAdapter"]
        FA["📁 File\nAdapter"]
        GA["🐙 Git\nAdapter"]
        SHA["🖥️  Shell\nAdapter"]
        DQ["🗄️  DB Query\nAdapter"]
        IA["🔌 Internal API\nAdapter"]
    end

    %% ── State & Memory Layer ──────────────────────────────────────
    subgraph MEM["💾  State & Memory Layer"]
        direction LR
        CH["💬 Conversation\nHistory"]
        TS["📊 Task\nState"]
        LTM["🧠 Long-term\nMemory"]
        VEC["🔎 Vector\nIndex"]
        AL["📝 Audit\nLog"]
    end

    %% ── Evaluation & Governance Layer ────────────────────────────
    subgraph EVG["📊  Evaluation & Governance Layer"]
        direction LR
        RT["🔭 Run\nTracing"]
        CT["💰 Cost\nTracker"]
        LT["⏱️  Latency\nTracker"]
        EV["🧪 Eval\nSuite"]
        AB["🚧 Approval\nBoundary"]
        RP["🔄 Replay\nHarness"]
    end

    %% ── Connections ───────────────────────────────────────────────
    EXP  -->|"requests"| ORC
    ORC  -->|"tool calls"| TH
    ORC  -->|"read / write state"| MEM
    ORC  -->|"trace / eval / gate"| EVG

    %% ── Styling ───────────────────────────────────────────────────
    classDef expStyle   fill:#dbeafe,stroke:#3b82f6,color:#1e3a5f
    classDef orcStyle   fill:#ede9fe,stroke:#7c3aed,color:#2e1065
    classDef toolStyle  fill:#dcfce7,stroke:#16a34a,color:#14532d
    classDef memStyle   fill:#fef9c3,stroke:#ca8a04,color:#713f12
    classDef evalStyle  fill:#ffe4e6,stroke:#e11d48,color:#4c0519

    class WEB,TG,ADM,CLI expStyle
    class SC,TR2,PL,SM,APR2 orcStyle
    class WS,FA,GA,SHA,DQ,IA toolStyle
    class CH,TS,LTM,VEC,AL memStyle
    class RT,CT,LT,EV,AB,RP evalStyle
```

---

## 5. Technology Stack Table

Bảng dưới tóm tắt **loại công nghệ** và **team chịu trách nhiệm** cho từng layer.  
Chi tiết lựa chọn cụ thể (version, config) xem tài liệu `02b_infrastructure_setup.md`.

| Layer | Vai trò chính | Công nghệ tiêu biểu | Owner |
|---|---|---|---|
| **Experience** | UI, realtime channel, bot | Angular, SignalR, Telegram Bot API | Frontend / Product |
| **Orchestration** | API, session, background worker | ASP.NET Core, .NET BackgroundService | Backend |
| **Tool Harness** | Adapter registry, tool execution | ASP.NET Core + JSON Schema validation | Backend / Platform |
| **State & Memory** | Persistent state, cache, vector search | PostgreSQL, Redis, pgvector / Qdrant | Platform / Data |
| **Evaluation & Governance** | Tracing, metrics, audit log | OpenTelemetry, Prometheus, Grafana | SRE / Platform |
| **LLM Integration** | Model adapter, prompt dispatch | OpenAI SDK / Azure OpenAI SDK | AI/ML Engineer |

---

## 6. Khi nào dùng LLM, khi nào dùng code

> **Nguyên tắc cốt lõi:** Dùng code cho mọi thứ có thể xác định trước (deterministic). Dùng LLM chỉ khi cần reasoning, language understanding, hoặc judgment mà code không thể hardcode được.

### Heuristic nhanh

| Câu hỏi | Trả lời → dùng |
|---|---|
| Output có thể viết thành `if/else` hay rule rõ ràng không? | ✅ Code |
| Output phụ thuộc vào ngữ nghĩa / ngữ cảnh tự nhiên không? | ✅ LLM |
| Sai lầm ở đây có side effect nghiêm trọng không (xoá dữ liệu, gửi tiền...)? | ✅ Code kiểm soát, LLM chỉ đề xuất |
| Cần kết quả 100% nhất quán và auditable không? | ✅ Code |

### Ví dụ áp dụng trong hệ thống

| Tình huống | Xử lý bằng |
|---|---|
| Validate input schema, enforce timeout, check permission | Code |
| Route task theo rule cố định, tính cost/latency/token | Code |
| Ghi audit log, persist state, trigger approval gate | Code |
| Phân loại intent của user request | LLM |
| Decompose task thành các bước, sinh execution plan | LLM |
| Tổng hợp kết quả từ nhiều tool, viết summary cho người dùng | LLM |
| Đánh giá chất lượng output (model-graded eval) | LLM |

> Chi tiết decision framework cho từng use case xem `02c_llm_vs_code_decision_guide.md`.
