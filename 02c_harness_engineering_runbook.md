# 02c — Harness Engineering: Implementation Runbook & Checklist

## 1. Mục đích tài liệu

Tài liệu này là runbook triển khai thực tế — bao gồm roadmap 90 ngày, task table từng phase, checklist POC/Beta/Production, cấu trúc code, danh sách màn hình Angular, bộ eval, và các sai lầm cần tránh.
Dùng trực tiếp để implement và vận hành — không phải để đọc cho hiểu.

---

## 2. 90-Day Roadmap Table

| Giai đoạn | Ngày | Mục tiêu | Deliverable chính |
|---|---|---|---|
| Phase 1 — Skeleton | 1–14 | Harness tối thiểu nhưng chuẩn nền | Gửi message, tạo run, gọi tool, log step vào DB |
| Phase 2 — Orchestration | 15–30 | Router, planner, state machine, approval | Multi-step task ổn định, approval card hoạt động |
| Phase 3 — Memory & Reliability | 31–50 | Memory tách lớp, background execution, resilience | Run dài không block thread, resume khi restart |
| Phase 4 — Eval & Observability | 51–70 | Trace, metrics, eval suite, cost tracking | Biết thay đổi nào làm hệ tốt hay tệ hơn |
| Phase 5 — Production Hardening | 71–90 | Multi-tenant, security, quota, audit | Sẵn sàng mời user thật vào thử |

---

## 3. Phase 1 Task Table (ngày 1–14)

| Task | Mô tả | Component | Acceptance Criteria |
|---|---|---|---|
| Tạo .NET solution | Scaffold `Harness.Api`, `Harness.Application`, `Harness.Domain`, `Harness.Infrastructure`, `Harness.Worker`, `Harness.Contracts` | Tất cả | Build thành công, không có circular dependency |
| Dựng PostgreSQL schema | Tạo migration cho `sessions`, `messages`, `runs`, `run_steps`, `approvals` | Infrastructure | Migration chạy được, FK constraints đúng |
| Setup Redis | Cache, distributed lock, pub/sub | Infrastructure | Kết nối thành công, lock acquire/release hoạt động |
| Implement tool: `web_search` | Gọi search API, validate input/output schema | Tool Harness | Trả kết quả đúng schema, timeout hoạt động |
| Implement tool: `file_read` | Đọc file từ workspace, validate path | Tool Harness | Đọc được file, từ chối path traversal |
| Implement tool: `file_write` | Ghi file với approval gate | Tool Harness | Tạo approval request trước khi ghi |
| Implement 3 execution strategy | Direct answer, Tool-assisted, Plan-and-execute đơn giản | Orchestration | Mỗi strategy chạy được end-to-end |
| Thêm SignalR hub | Stream trạng thái run về UI | Infrastructure | Client nhận được event khi step thay đổi |
| Angular: màn chat cơ bản | Gửi message, nhận realtime update | Experience | User gõ message → nhận answer qua SignalR |

---

## 4. Phase 2 Task Table (ngày 15–30)

| Task | Mô tả | Component | Acceptance Criteria |
|---|---|---|---|
| Task classifier | Phân loại intent thành task_type | Orchestration | Classifier trả đúng type cho 10 case test |
| Strategy router | Chọn strategy dựa trên task_type và config | Orchestration | Rule-based routing hoạt động, có fallback |
| State machine cho run | Implement đầy đủ 10 state, transition rõ ràng | Domain | Không có transition không hợp lệ, state persist vào DB |
| Approval workflow | CreateApprovalRequest, wait, resume/cancel | Orchestration + Infrastructure | Approve → run tiếp; Deny → run cancelled; TTL → auto cancel |
| Error model chuẩn hóa | `validation_error`, `permission_denied`, `timeout`, `dependency_failed`, `tool_unavailable` | Tất cả | Tất cả lỗi trả đúng error_code, không lộ stack trace ra client |
| Angular: Session List | Danh sách sessions với status và last activity | Experience | Hiển thị đúng, filter theo status hoạt động |
| Angular: Run Detail | Timeline step, tool result, token usage, cost | Experience | Xem được toàn bộ trace của một run |
| Angular: Approval Queue | Pending approvals, payload preview, approve/deny button | Experience | Approve/deny từ UI, run resume/cancel tương ứng |

---

## 5. Phase 3 Task Table (ngày 31–50)

| Task | Mô tả | Component | Acceptance Criteria |
|---|---|---|---|
| Memory: conversation | Lưu và lấy lịch sử hội thoại theo session | State & Memory | Context đúng khi hỏi follow-up question |
| Memory: working | Scratchpad per-run trong Redis, TTL khi run kết thúc | State & Memory | Không rò rỉ working memory giữa các run |
| Memory: long-term | Lưu preference và facts vào `memories` table | State & Memory | Recall đúng sau khi session mới |
| Retry policy per tool | Exponential backoff, max_attempts, retry_on | Tool Harness | Tool timeout → retry đúng số lần → fail step |
| Circuit breaker | Cho LLM provider và external API | Infrastructure | Sau N lỗi liên tiếp, circuit mở, fail fast |
| Checkpoint / resume | Lưu checkpoint sau mỗi step, resume từ checkpoint | Orchestration | Kill worker → restart → run tiếp từ step chưa hoàn thành |
| Queue consumer | Background execution qua queue | Worker | Run không block API thread, queue đảm bảo at-least-once |
| Rate limit per user/tenant | Giới hạn số run đồng thời và token/ngày | Infrastructure | Vượt limit → 429, không affect user khác |

---

## 6. Phase 4 Task Table (ngày 51–70)

| Task | Mô tả | Component | Acceptance Criteria |
|---|---|---|---|
| OpenTelemetry tracing | Trace span cho mọi run, step, tool call | Infrastructure | Jaeger/Tempo hiển thị trace end-to-end |
| Prometheus metrics | Export metrics: run count, latency, token, cost, error rate | Infrastructure | Dashboard Grafana có đủ 8 metric tối thiểu |
| Bộ eval nội bộ 50 case | Test case cố định cho regression test | Evaluation | CI chạy eval, report pass/fail rate |
| Replay tool | Replay bất kỳ run cũ với input gốc | Evaluation | Replay cho ra output có thể so sánh với original |
| Cost tracking | Ghi cost theo model, user, tenant, feature | Infrastructure | Query được "tháng này tốn bao nhiêu theo từng tenant" |
| Angular: Eval Dashboard | Recent regressions, failed cases, latency trend, cost trend | Experience | Sau mỗi deploy có thể thấy impact ngay |

---

## 7. Phase 5 Task Table (ngày 71–90)

| Task | Mô tả | Component | Acceptance Criteria |
|---|---|---|---|
| Multi-tenant permission matrix | Role × tool × action → allow/deny | Domain + Infrastructure | Tenant A không gọi được tool của Tenant B |
| Secret management | Vault / Azure Key Vault, không hardcode credential | Infrastructure | Không có credential nào trong source code hoặc DB unencrypted |
| Quota per team/customer | Giới hạn token, run, cost theo tenant | Infrastructure | Vượt quota → block với message rõ ràng |
| Immutable audit log | Ghi audit cho mọi external write action | Infrastructure | Audit log không thể sửa hoặc xóa qua API thường |
| Sandbox cho shell/file | Chạy shell action trong container hoặc chroot | Tool Harness | Shell action không thoát ra ngoài sandbox |
| Background janitor job | Cleanup expired artifacts, archive old traces, compact memory | Worker | Chạy định kỳ, không ảnh hưởng production traffic |
| On-call runbook | Tài liệu xử lý 5 incident phổ biến nhất | Ops | Mỗi incident có steps rõ, contact rõ, escalation rõ |

---

## 8. POC Checklist

| Item | Mô tả | Done? |
|---|---|---|
| UI nhận chat | Angular chat UI gửi được message và nhận realtime response | ☐ |
| Orchestrator service | Classify + route + execute ít nhất 1 strategy | ☐ |
| 3 tool có schema | `web_search`, `file_read`, `file_write` với input/output schema | ☐ |
| Database lưu runs và steps | `sessions`, `runs`, `run_steps` có data sau mỗi conversation | ☐ |
| Approval flow | `file_write` trigger approval, user approve/deny từ UI | ☐ |
| Dashboard xem traces | Xem được step timeline của 1 run bất kỳ trong Admin UI | ☐ |

---

## 9. Beta Checklist

| Item | Mô tả | Done? |
|---|---|---|
| Queue + worker | Run dài chạy background, không block API thread | ☐ |
| Retry + timeout + circuit breaker | Tool fail → retry; circuit mở khi provider down | ☐ |
| Memory separation | Conversation / working / long-term memory tách rõ | ☐ |
| Eval suite | 50 test case chạy được trong CI | ☐ |
| Cost tracking | Query được cost theo user và model | ☐ |
| Replay run | Replay bất kỳ run lỗi để debug | ☐ |

---

## 10. Production Checklist

| Item | Mô tả | Done? |
|---|---|---|
| Multi-tenant authz | Tenant isolation ở cả API và database layer | ☐ |
| Audit log | Immutable log cho mọi external write | ☐ |
| Secret management | Credential qua Vault / Key Vault, không hardcode | ☐ |
| Tool permission matrix | Mỗi tool có required_permissions, check trước khi execute | ☐ |
| SLO dashboard | p50/p95 latency, error rate, cost/day hiển thị realtime | ☐ |
| On-call runbook | Ít nhất 5 incident scenario có hướng xử lý rõ | ☐ |

---

## 11. Code Structure Table

| Project | Layer | Chứa gì |
|---|---|---|
| `Harness.Domain` | Domain | Session aggregate, Run aggregate, Approval aggregate, ToolDefinition, domain events, policies |
| `Harness.Application` | Application | Use cases, orchestrator interfaces, strategy selection, command handlers, DTOs |
| `Harness.Infrastructure` | Infrastructure | EF Core repositories, provider adapters, queue adapters, search adapters, file system adapters, observability integrations |
| `Harness.Api` | Presentation | REST API controllers, SignalR hubs, webhook endpoints, auth filters |
| `Harness.Worker` | Worker | Long-running executor, queue consumers, scheduler jobs, retry processor |
| `Harness.Contracts` | Shared | Tool schemas, event contracts, error codes, permission constants |

---

## 12. Angular Admin UI Screen List

| Màn hình | Component chính | Data cần hiển thị |
|---|---|---|
| Session List | `SessionListComponent` | session_id, user, channel, last_activity_at, status, active run count |
| Run Detail | `RunDetailComponent`, `StepTimelineComponent` | input, strategy, model, step timeline, tool results, token usage, cost_usd, final answer |
| Approval Queue | `ApprovalQueueComponent`, `ApprovalCardComponent` | pending approvals, action_type, requested_payload preview, risk level, approve/deny buttons, resolved history |
| Eval Dashboard | `EvalDashboardComponent` | recent regression failures, failed case list, latency trend chart, cost trend chart, top failing tools |
| Tool Registry | `ToolRegistryComponent` | tool list, schema viewer, side_effect_level, timeout, retry_policy |
| Memory Explorer | `MemoryExplorerComponent` | memory type filter, scope, content preview, importance_score, created_at, TTL |

---

## 13. Evaluation Suite Table

| Loại eval | Số case | Trigger | Rubric chấm |
|---|---|---|---|
| Regression eval | 50–200 | Mỗi commit thay đổi prompt, tool, hoặc router | Pass/fail theo golden output; so sánh token count và latency |
| Task success eval | 30 | Mỗi release | Research: tìm đúng nguồn; Coding: sửa đúng file; Ops: tạo approval đúng |
| Safety eval | 20 | Mỗi release | Không tự gọi external write khi chưa approve; không lộ dữ liệu ngoài scope; không gọi sai permission |
| Cost regression eval | 10 | Mỗi khi đổi model hoặc strategy | Token count không tăng quá 20% so với baseline cùng task |
| Latency eval | 10 | Mỗi release | p95 latency trong SLO; không có step nào > 2× median |

---

## 14. Common Mistakes Table

| Sai lầm | Hậu quả | Cách tránh |
|---|---|---|
| Bắt đầu bằng prompt trước khi có state model | Run không trace được, không replay được, không debug được | Thiết kế `runs` + `run_steps` schema trước khi viết prompt |
| Nhét quá nhiều quyền vào agent quá sớm | Agent tự gọi external API gây incident thật | Mặc định tất cả tool là `read_only`, mở rộng có kiểm soát |
| Để LLM quyết định phần deterministic | Chi phí cao, không ổn định, khó test | Dùng code cho validation, routing rule, timeout, retry, permission check |
| Không log tool call | Không debug được khi fail, không có data để eval | Mọi tool call đều phải persist vào `run_steps` |
| Không có dataset eval, chỉ test bằng cảm giác | Không biết thay đổi nào làm hệ tốt hay tệ hơn | Tạo 50 golden case ngay từ Phase 4 |
| Trộn lẫn UI logic, orchestration, tool, và persistence | Code không test được, không scale được | Clean architecture: Domain → Application → Infrastructure → Presentation |
| Không version schema của tool | Tool update phá contract cũ, run cũ không replay được | Thêm `version` field vào `tool_definitions`, versioned endpoint |
| Dùng vector DB cho mọi thứ | Over-engineering, chi phí cao, latency cao | Chỉ dùng vector search khi cần semantic recall; PostgreSQL full-text search thường đủ |
| Xài multi-agent quá sớm | Tốn tiền, tăng latency, tăng complexity không cần thiết | Chỉ dùng multi-agent sau khi single-agent harness đã ổn định |
| Không có approval cho external write | Agent tự gửi email, push code, gọi API production | Mọi tool có `side_effect_level != read_only` đều phải qua approval gate |
