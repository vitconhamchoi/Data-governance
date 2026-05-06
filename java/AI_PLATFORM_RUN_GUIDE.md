# AI-Powered Data Governance (Java) Run Guide

## 1) Required environment variables

- `AI_LLM_API_KEY` (required, real key)
- `AI_LLM_BASE_URL` (optional, default OpenAI)
- `AI_LLM_MODEL` (default `gpt-4o-mini`)
- `AI_EMBEDDING_MODEL` (default `text-embedding-3-small`)
- `DATAHUB_TOKEN` (optional, if DataHub auth enabled)

## 2) Start platform

```bash
cd /home/runner/work/Data-governance/Data-governance/java
AI_LLM_API_KEY=<your-real-key> docker compose up -d --build
```

Services:
- policy-service: `http://localhost:8082`
- query-gateway: `http://localhost:8083`
- ai-classifier: `http://localhost:8084`
- ai-assistant: `http://localhost:8085`
- Trino: `http://localhost:8080`
- DataHub GraphQL endpoint: `http://localhost:9002/api/graphql`

## 3) Demo scenario

### A. Run AI classifier (regex + LLM fallback)

```bash
curl -X POST http://localhost:8084/ai/classify \
  -H "Content-Type: application/json" \
  -d '{"dataset":"users","datasetUrn":"urn:li:dataset:(urn:li:dataPlatform:trino,users,PROD)","sampleLimit":20}'
```

Expected: detects tags like `PII.email`, `PII.phone` and pushes DataHub tag mutation + policy recommendation request.

### B. Review and approve suggested policy

```bash
curl http://localhost:8082/policy-suggestions

curl -X POST http://localhost:8082/policy-suggestions/1/approve \
  -H "Content-Type: application/json" \
  -d '{"approvedBy":"governance-admin","role":"analyst"}'
```

### C. NL2SQL + governed execution

```bash
curl -X POST http://localhost:8083/query/nl2sql \
  -H "Content-Type: application/json" \
  -d '{"question":"total orders by month","role":"analyst"}'

curl -X POST http://localhost:8083/query/nl \
  -H "Content-Type: application/json" \
  -d '{"question":"show user emails","role":"analyst"}'
```

Expected: analyst sees masked email/phone based on approved policy.

### D. Semantic index + search + copilot

```bash
curl -X POST http://localhost:8085/ai/index \
  -H "Content-Type: application/json" \
  -d '{"dataset":"urn:li:dataset:(urn:li:dataPlatform:trino,orders,PROD)","description":"customer payment and order transactions"}'

curl "http://localhost:8085/ai/search?q=customer%20payment%20data&topK=5"

curl -X POST http://localhost:8085/ai/ask \
  -H "Content-Type: application/json" \
  -d '{"question":"Which tables contain PII?"}'
```

## 4) Notes

- Real AI calls are required; without valid `AI_LLM_API_KEY`, AI endpoints return error.
- DataHub integration uses GraphQL endpoint and tag mutation.
- Vector search uses `pgvector` (`vector(1536)` + cosine distance).
