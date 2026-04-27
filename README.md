# Kiến trúc dữ liệu phục vụ xây dựng AI trong doanh nghiệp

Tài liệu này tóm tắt một mô hình kiến trúc dữ liệu thực dụng để triển khai AI trong doanh nghiệp lớn, đặc biệt phù hợp với bối cảnh VNPT, ngân hàng, và doanh nghiệp nhà nước.

## 1. Mục tiêu của kiến trúc

Kiến trúc này không bắt đầu từ model. Nó bắt đầu từ 3 nền tảng:

- Dữ liệu đủ gom và đủ sạch
- Governance đủ chặt để lên production
- Lớp dữ liệu dùng chung đủ tốt để phục vụ BI, ML và GenAI cùng lúc

Mục tiêu là giúp doanh nghiệp:

- Tận dụng dữ liệu nghiệp vụ và dữ liệu phi cấu trúc cho AI
- Rút ngắn thời gian làm PoC đến production
- Kiểm soát chất lượng, bảo mật, lineage và audit
- Tạo ra use case AI có ROI rõ, thay vì chỉ dừng ở demo

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

## 3. Giải thích ngắn theo từng lớp

### 3.1. Nguồn dữ liệu

Bao gồm 4 nhóm chính:

- Hệ thống nghiệp vụ lõi như CRM, ERP, Billing, kế toán, nhân sự
- Dữ liệu phi cấu trúc như công văn, quy trình, email, hợp đồng, PDF scan
- Dữ liệu vận hành realtime như app log, call center, event stream, ticket
- Dữ liệu ngoài như open data, dữ liệu đối tác, dữ liệu điều tiết hoặc quy định

Đây là đầu vào cho toàn bộ hệ AI. Nếu chỉ có dữ liệu cấu trúc mà bỏ qua kho văn bản nội bộ, doanh nghiệp sẽ mất phần giá trị lớn nhất cho GenAI.

### 3.2. Tầng tích hợp dữ liệu

Tầng này nhận dữ liệu qua nhiều cơ chế:

- ETL hoặc ELT theo batch
- CDC từ hệ thống giao dịch
- API sync từ hệ thống ngoài
- Streaming hoặc message bus cho dữ liệu thời gian thực

Vai trò chính là gom dữ liệu về một trục chung, giảm tình trạng hệ nào biết hệ đó.

### 3.3. Data Lakehouse doanh nghiệp

Đây là nơi hợp nhất dữ liệu để dùng lại cho nhiều mục tiêu:

- BI truyền thống
- Phân tích nâng cao
- Huấn luyện ML
- RAG và GenAI

Thường chia thành các zone:

- Raw: dữ liệu gốc
- Standardized/Clean: dữ liệu đã chuẩn hóa
- Curated/Trusted: dữ liệu đã kiểm soát chất lượng và sẵn sàng phục vụ nghiệp vụ

### 3.4. Xử lý và chuẩn hóa dữ liệu

Lớp này xử lý các vấn đề khiến AI thất bại khi lên production:

- Thiếu chuẩn dữ liệu
- Trùng lặp dữ liệu khách hàng hoặc hồ sơ
- Sai định nghĩa chỉ tiêu giữa các đơn vị
- Thiếu data quality rule

Thành phần thường có:

- Data Quality
- MDM
- Deduplication
- Mapping chỉ tiêu và chuẩn hóa nghiệp vụ

### 3.5. Governance và Security

Đây là phần bắt buộc với doanh nghiệp lớn, nhất là ngân hàng và DNNN.

Các năng lực cần có:

- Data catalog
- Data lineage
- IAM và phân quyền
- Masking dữ liệu nhạy cảm
- Audit log
- Compliance theo chính sách nội bộ và quy định pháp lý

Nếu governance yếu, doanh nghiệp có thể demo AI được nhưng rất khó cho vận hành thật.

### 3.6. Lớp dữ liệu dùng chung

Đây là lớp tạo giá trị dùng lại cho nhiều bài toán khác nhau:

- Semantic layer để thống nhất khái niệm KPI và chỉ tiêu
- Data mart phục vụ phân tích
- Feature data cho ML
- Knowledge base cho GenAI và RAG

Lớp này giúp tránh việc mỗi team tự kéo dữ liệu riêng và tạo ra nhiều “sự thật” khác nhau.

### 3.7. Hai nhánh AI chính

#### a. AI phân tích dự báo

Phù hợp cho các bài toán như:

- Dự báo doanh thu
- Phân tích churn
- Phát hiện bất thường
- Chấm điểm rủi ro
- Dự báo nhu cầu

#### b. GenAI và RAG nội bộ

Phù hợp cho:

- Hỏi đáp tài liệu nội bộ
- Tra cứu quy trình
- Tóm tắt báo cáo dài
- Hỗ trợ nhân sự mới tiếp cận tri thức doanh nghiệp
- Trợ lý tìm kiếm văn bản và tổng hợp thông tin

### 3.8. AI Platform, MLOps, LLMOps

Đây là tầng giúp AI vận hành như một sản phẩm thật, không phải demo:

- Quản lý training và evaluation
- Model registry
- Prompt management
- CI/CD cho AI
- Monitoring chất lượng, chi phí, latency, drift
- Human feedback loop khi cần

### 3.9. Ứng dụng nghiệp vụ

Các use case nên ưu tiên trước trong bối cảnh doanh nghiệp Việt Nam:

- Trợ lý hỏi đáp công văn, quy trình, tài liệu nội bộ
- OCR và trích xuất hồ sơ, chứng từ
- Tóm tắt báo cáo điều hành
- Phân tích khách hàng và vận hành
- Agent hỗ trợ tác nghiệp hẹp, có kiểm soát

## 4. Lộ trình triển khai thực dụng

### Giai đoạn 1. Xây nền

- Gom dữ liệu trọng yếu
- Chuẩn hóa chất lượng dữ liệu
- Thiết lập catalog, lineage, phân quyền
- Dựng lakehouse hoặc nền dữ liệu tập trung

### Giai đoạn 2. Tạo lớp dùng chung

- Xây semantic layer
- Tạo curated datasets
- Tạo knowledge base nội bộ
- Chuẩn bị feature-ready data

### Giai đoạn 3. Chọn use case AI có ROI rõ

Ưu tiên các use case:

- Đầu vào lặp lại nhiều
- Quy trình rõ
- Có thể đo tiết kiệm thời gian hoặc nhân công
- Có human review nếu độ chính xác chưa tuyệt đối

### Giai đoạn 4. Mở rộng có kiểm soát

- Mở từ hỏi đáp tài liệu sang copilots nghiệp vụ
- Mở từ predictive AI sang agent hỗ trợ tác nghiệp
- Gắn monitoring, audit và cost control ngay từ đầu

## 5. Các ngách AI dễ ra tiền để nuôi được chi phí API

Nếu tiêu chí là doanh thu phải gánh được tiền API, thì các ngách sau đáng làm hơn chatbot chung chung:

### 5.1. OCR và trích xuất hồ sơ, chứng từ

Phù hợp với:

- Ngân hàng
- Bảo hiểm
- Kế toán
- Logistics
- Hành chính doanh nghiệp

Lý do đáng làm:

- ROI rõ
- Khách hàng quen trả tiền cho năng suất và giảm lỗi
- Có thể bán theo số lượng hồ sơ hoặc theo gói xử lý

### 5.2. AI knowledge assistant nội bộ

Phù hợp với doanh nghiệp có nhiều:

- Quy trình
- Công văn
- Tài liệu đào tạo
- Tài liệu kỹ thuật

Mô hình doanh thu:

- Thuê bao theo user
- Thuê bao theo phòng ban
- Dịch vụ triển khai + phí duy trì

### 5.3. AI copilot cho vận hành và call center

Phù hợp để:

- Gợi ý trả lời
- Tóm tắt cuộc gọi
- Kiểm tra tuân thủ
- Phân loại ticket

Giá trị chính là tăng productivity, giảm AHT, giảm thời gian xử lý, tăng tính nhất quán.

## 6. Kết luận

Điểm chốt quan trọng nhất là:

- AI không nên bắt đầu từ model
- AI nên bắt đầu từ dữ liệu, governance, và use case có ROI đo được
- Với doanh nghiệp Việt Nam, đường đi khôn ngoan là làm RAG, OCR, dashboard augmentation, và copilot hẹp trước
- Agent tổng quát hoặc chatbot đại trà thường đẹp ở demo nhưng khó nuôi bằng doanh thu thật

Nếu cần trình bày với lãnh đạo trong 30 giây, có thể chốt 1 câu:

> Thứ đáng đầu tư nhất không phải model, mà là nền dữ liệu, governance, và lớp dữ liệu dùng chung để AI phục vụ được bài toán kinh doanh thật.
