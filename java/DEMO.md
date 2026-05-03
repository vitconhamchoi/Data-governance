# Data Governance System - Demo Guide

## Prerequisites
- Docker and Docker Compose installed
- At least 8GB RAM available
- Ports 5432, 8080, 8081, 8082, 8083, 9000, 9001 available

## Step 1: Start the System

```bash
cd java
docker-compose up -d
```

Wait for all services to be healthy (about 2-3 minutes):
```bash
docker-compose ps
```

## Step 2: Verify Services

| Service | URL | Credentials |
|---------|-----|-------------|
| Trino | http://localhost:8080 | admin / (no password) |
| Airflow | http://localhost:8081 | admin / admin123 |
| MinIO | http://localhost:9001 | minioadmin / minioadmin123 |
| Policy Service | http://localhost:8082 | - |
| Query Gateway | http://localhost:8083 | - |

## Step 3: Load Sample Data

Data is auto-loaded via init.sql on PostgreSQL startup. Verify:

```bash
docker exec -it dg-postgres psql -U dguser -d datagovernance -c "SELECT * FROM users;"
```

## Step 4: Run Airflow Pipeline

### Option A: Via UI
1. Open http://localhost:8081
2. Login: admin / admin123
3. Enable DAG: `data_governance_pipeline`
4. Trigger manually

### Option B: Via CLI
```bash
docker exec dg-airflow airflow dags trigger data_governance_pipeline
```

## Step 5: Verify Data Quality (Soda)

```bash
docker exec dg-airflow soda scan \
  -d postgres_default \
  -c /opt/airflow/soda/soda_config.yml \
  /opt/airflow/soda/checks/users_checks.yml
```

## Step 6: Ingest Metadata to DataHub

```bash
docker exec dg-airflow datahub ingest -c /opt/airflow/datahub/ingestion_recipe.yml
```

## Step 7: Policy Management

### List all policies
```bash
curl -s http://localhost:8082/policies | jq .
```

### Create a masking policy for analyst role
```bash
curl -X POST http://localhost:8082/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "columnName": "email",
    "rule": "MASK",
    "role": "analyst"
  }'
```

```bash
curl -X POST http://localhost:8082/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "columnName": "phone",
    "rule": "MASK",
    "role": "analyst"
  }'
```

### Delete a policy
```bash
curl -X DELETE http://localhost:8082/policies/1
```

## Step 8: Query with Role-Based Masking

### Query as admin (full data)
```bash
curl -X POST http://localhost:8083/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM postgresql.public.users",
    "role": "admin"
  }' | jq .
```

Expected response:
```json
{
  "columns": ["id", "name", "email", "phone"],
  "rows": [
    {"id": 1, "name": "Alice", "email": "alice@example.com", "phone": "0901234567"},
    {"id": 2, "name": "Bob", "email": "bob@example.com", "phone": "0912345678"}
  ],
  "rowCount": 5,
  "role": "admin",
  "masked": false
}
```

### Query as analyst (masked data)
```bash
curl -X POST http://localhost:8083/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM postgresql.public.users",
    "role": "analyst"
  }' | jq .
```

Expected response:
```json
{
  "columns": ["id", "name", "email", "phone"],
  "rows": [
    {"id": 1, "name": "Alice", "email": "a***@example.com", "phone": "****4567"},
    {"id": 2, "name": "Bob", "email": "b***@example.com", "phone": "****5678"}
  ],
  "rowCount": 5,
  "role": "analyst",
  "masked": true
}
```

## Step 9: Test Orders Query

```bash
curl -X POST http://localhost:8083/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT o.id, u.name, u.email, o.amount FROM postgresql.public.orders o JOIN postgresql.public.users u ON o.user_id = u.id",
    "role": "analyst"
  }' | jq .
```

## Step 10: Shutdown

```bash
docker-compose down -v
```
