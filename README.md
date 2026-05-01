# Kiến trúc dữ liệu và nền tảng AI trong doanh nghiệp

Repo này gom các tài liệu kiến trúc theo hướng **đọc từ tổng quan đến triển khai**, tránh tản mạn và trùng ý.

## Bố cục tài liệu

### 01. Tổng quan kiến trúc dữ liệu và AI doanh nghiệp
- **File:** `README.md`
- **Mục đích:** trình bày bức tranh tổng thể về data platform, governance, BI, ML, GenAI và RAG trong doanh nghiệp
- **Đọc khi:** cần hiểu khung lớn trước khi đi vào từng nhánh kỹ thuật cụ thể

### 02. Harness Engineering cho hệ thống AI production
- **File:** `02_harness_engineering.md`
- **Mục đích:** giải thích Harness Engineering là gì, gồm những thành phần nào, data model ra sao, state machine thế nào, và nên triển khai theo lộ trình nào
- **Đọc khi:** cần thiết kế hoặc review hệ thống AI agent / orchestration / tool-calling ở mức production

### 03. Kiến trúc IoMT cho hệ thống khám sức khỏe ở quy mô lớn
- **File:** `03_iomt_kien_truc_kham_suc_khoe_quy_mo_lon.md`
- **Mục đích:** mô tả kiến trúc IoMT theo hướng production-first cho bài toán khám sức khỏe, bao gồm kết nối thiết bị, ingestion, event backbone, processing, storage, alerting và HA/DR
- **Đọc khi:** cần thiết kế hệ thống khám sức khỏe có khả năng mở rộng lên hàng triệu người dùng hoặc hàng triệu thiết bị

---

## 1. Mục tiêu của kiến trúc tổng thể

Kiến trúc này không bắt đầu từ model. Nó bắt đầu từ 3 nền tảng:

- dữ liệu đủ gom và đủ sạch,
- governance đủ chặt để lên production,
- lớp dữ liệu dùng chung đủ tốt để phục vụ BI, ML và GenAI cùng lúc.

Mục tiêu là giúp doanh nghiệp:

- tận dụng dữ liệu nghiệp vụ và dữ liệu phi cấu trúc cho AI,
- rút ngắn thời gian từ PoC đến production,
- kiểm soát chất lượng, bảo mật, lineage và audit,
- tạo ra use case AI có ROI rõ thay vì chỉ dừng ở demo.

---

## 2. Mô hình kiến trúc tổng thể

```mermaid
flowchart LR
    A[Hệ thống nghiệp vụ lõi<br/>CRM, ERP, Billing, Kế toán, Nhân sự] --> D
    B[Hồ sơ/văn bản nội bộ<br/>Công văn, Quy trình, Hợp đồng, Email, PDF scan] --> D
    C[Kênh vận hành thời gian thực<br/>App, Web, Call Center, Network Log, Ticket] --> D
    X[Nguồn dữ liệu ngoài<br/>CSDL công, đối tác, open data, regulatory data] --> D

    D[Tầng tích hợp dữ liệu<br/>ETL/ELT, API, CDC, Message Bus, Streaming] --> E
    E[Data Lakehouse doanh nghiệp<br/>Raw, Standardized, Curated, Trusted] --> F
    E --> G

    F[Xử lý và chuẩn hóa dữ liệu<br/>Data Quality, MDM, Dedup, Mapping chỉ tiêu] --> H
    G[Governance và Security<br/>Catalog, Lineage, IAM, Masking, Audit, Compliance] --> H

    H[Lớp dữ liệu dùng chung<br/>Semantic Layer, Data Mart, Feature Data, Knowledge Base] --> I
    H --> J
    H --> M

    I[AI phân tích dự báo<br/>Churn, doanh thu, rủi ro, nhu cầu, bất thường] --> K
    J[GenAI và RAG nội bộ<br/>Chunking, Embedding, Vector Search, Retrieval] --> K
    M[BI và Dashboard điều hành<br/>Báo cáo quản trị, KPI, drill-down] --> K

    K[AI Platform, MLOps, LLMOps<br/>Evaluation, Registry, Prompt Management, Deployment, Monitoring] --> L

    L[Ứng dụng nghiệp vụ<br/>Trợ lý hỏi đáp văn bản<br/>Tóm tắt báo cáo<br/>OCR hồ sơ<br/>Phân tích khách hàng<br/>Agent hỗ trợ tác nghiệp]
```

---

## 3. Cách đọc repo này

Nếu đọc theo đúng logic, nên đi theo thứ tự:

1. **README.md** — hiểu khung dữ liệu và AI ở cấp doanh nghiệp
2. **02_harness_engineering.md** — hiểu lớp điều phối cho AI production
3. **03_iomt_kien_truc_kham_suc_khoe_quy_mo_lon.md** — hiểu bài toán IoMT cho khám sức khỏe ở quy mô production lớn

---

## 4. Kết luận

Bộ tài liệu này được tổ chức theo nguyên tắc:

- từ tổng quan đến chi tiết,
- từ logic nghiệp vụ đến kiến trúc kỹ thuật,
- từ mô hình khái niệm đến bài toán production.

Mục tiêu là để người đọc không phải ghép ý từ nhiều file rời rạc, mà có thể lần theo đúng mạch tư duy của một hệ thống thực tế.
