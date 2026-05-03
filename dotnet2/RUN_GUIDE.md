# Data Governance System - Run Guide

## Overview

This is a **production-ready Data Governance system** featuring:

- **Metadata catalog**: DataHub OSS
- **Pipeline orchestration**: Apache Airflow + dbt
- **Query engine**: Trino
- **Database**: PostgreSQL
- **Object storage**: MinIO
- **Data quality**: Soda Core (integrated in Airflow)
- **Policy enforcement**: .NET 8 ASP.NET Core Web APIs

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Data Sources                         │
│                 CSV Files → PostgreSQL                  │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Apache Airflow + dbt                       │
│         - Data ingestion pipeline                       │
│         - Data transformations                          │
│         - Quality checks (Soda Core)                    │
│         - Lineage emission to DataHub                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                  PostgreSQL                             │
│              users, orders tables                       │
│            (PII: email, phone marked)                   │
└─────────────┬──────────────────────────────────────────┘
              │
┌─────────────▼─────────────┐  ┌──────────────────────────┐
│         Trino             │  │    DataHub               │
│   Distributed Query       │  │   Metadata Catalog       │
│       Engine              │  │   + Lineage              │
└─────────────┬─────────────┘  └──────────────────────────┘
              │
┌─────────────▼────────────────────────────────────────────┐
│              Query Gateway (.NET 8)                      │
│  - Receives SQL + user role                             │
│  - Fetches policies from Policy Service                 │
│  - Executes query via Trino                             │
│  - Applies masking based on role                        │
└──────────────────────────────────────────────────────────┘
              │
┌─────────────▼────────────────────────────────────────────┐
│            Policy Service (.NET 8)                       │
│  - CRUD operations for policies                         │
│  - Stores: dataset, column, rule, role                  │
│  - PostgreSQL storage with EF Core                      │
└──────────────────────────────────────────────────────────┘
```

## Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- 8GB RAM minimum (16GB recommended)
- curl (for testing)
- jq (optional, for pretty JSON output)

## Quick Start

### 1. Start the System

```bash
cd dotnet2
chmod +x scripts/setup.sh
./scripts/setup.sh
```

This will:
- Start all Docker containers
- Initialize databases
- Create sample data (users, orders)
- Wait for services to be ready

### 2. Run the Demo

```bash
chmod +x scripts/demo.sh
./scripts/demo.sh
```

This will:
- Create masking policies for analyst role
- Query as admin (full data)
- Query as analyst (masked PII)
- Show all policies

## Services & Access

| Service | URL | Credentials |
|---------|-----|-------------|
| **Airflow** | http://localhost:8081 | admin/admin |
| **Trino** | http://localhost:8080 | - |
| **DataHub** | http://localhost:9002 | - |
| **MinIO Console** | http://localhost:9001 | minioadmin/minioadmin123 |
| **Policy API** | http://localhost:5001/swagger | - |
| **Query Gateway** | http://localhost:5002/swagger | - |
| **PostgreSQL** | localhost:5432 | datauser/datapass123 |

## Step-by-Step Demo Scenario

### 1. Check Initial Data

Query the database to see raw data:

```bash
docker exec -it postgres psql -U datauser -d datagovernance -c "SELECT * FROM users;"
```

### 2. Create Policies

Create a policy to mask email for analysts:

```bash
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "email",
    "rule": "mask",
    "role": "analyst"
  }'
```

Create a policy to mask phone for analysts:

```bash
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "phone",
    "rule": "mask",
    "role": "analyst"
  }'
```

### 3. Query as Admin (No Masking)

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM postgresql.public.users LIMIT 3",
    "role": "admin",
    "dataset": "users"
  }' | jq '.'
```

**Expected output:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john.doe@example.com",
      "phone": "+1-555-0101"
    }
  ],
  "columns": ["id", "name", "email", "phone"],
  "rowCount": 3,
  "success": true,
  "appliedPolicies": []
}
```

### 4. Query as Analyst (With Masking)

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM postgresql.public.users LIMIT 3",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.'
```

**Expected output:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "j***@example.com",
      "phone": "****0101"
    }
  ],
  "columns": ["id", "name", "email", "phone"],
  "rowCount": 3,
  "success": true,
  "appliedPolicies": ["email:mask", "phone:mask"]
}
```

### 5. View DataHub Lineage

1. Open http://localhost:9002
2. Navigate to Datasets
3. Search for "users" or "orders"
4. View lineage graph showing:
   - CSV source
   - dbt transformations
   - Target tables

### 6. Run Data Quality Pipeline

1. Open Airflow: http://localhost:8081
2. Login with admin/admin
3. Find DAG: `data_governance_pipeline`
4. Enable and trigger the DAG
5. Monitor execution:
   - dbt models run
   - Data quality checks execute
   - Lineage emitted to DataHub

Check quality results:

```bash
docker exec -it postgres psql -U datauser -d datagovernance \
  -c "SELECT * FROM data_quality_checks ORDER BY checked_at DESC LIMIT 5;"
```

### 7. Test Different Policy Rules

Create a "deny" policy:

```bash
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "phone",
    "rule": "deny",
    "role": "viewer"
  }'
```

Query as viewer:

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM postgresql.public.users LIMIT 1",
    "role": "viewer",
    "dataset": "users"
  }' | jq '.'
```

Phone will show `[REDACTED]` instead of masked value.

## API Examples

### Policy Service API

**Get all policies:**
```bash
curl http://localhost:5001/api/policies | jq '.'
```

**Get policies for specific dataset and role:**
```bash
curl http://localhost:5001/api/policies/dataset/users/role/analyst | jq '.'
```

**Delete a policy:**
```bash
curl -X DELETE http://localhost:5001/api/policies/1
```

### Query Gateway API

**Execute query with role-based access:**
```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email FROM postgresql.public.users WHERE id = 1",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.'
```

## Data Quality Checks

The Airflow DAG includes the following checks:

1. **Users table row count > 0**
2. **Users table - no null emails**
3. **Orders table row count > 0**
4. **Orders table - no null user_id**

If any check fails, the pipeline stops and logs the failure.

## Masking Rules

### Email Masking
- Input: `john.doe@example.com`
- Output: `j***@example.com`

### Phone Masking
- Input: `+1-555-0101`
- Output: `****0101`

### Deny Rule
- Any value → `[REDACTED]`

## Sample Datasets

### Users Table
- **Columns**: id, name, email, phone, created_at
- **PII Markers**: email, phone
- **Sample data**: 5 users

### Orders Table
- **Columns**: id, user_id, amount, created_at
- **Sample data**: 6 orders

## Troubleshooting

### Services not starting

```bash
docker-compose down -v
docker-compose up -d
```

### Check logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f policy-service
docker-compose logs -f query-gateway
docker-compose logs -f airflow-webserver
```

### Reset database

```bash
docker exec -it postgres psql -U datauser -d datagovernance -f /docker-entrypoint-initdb.d/init-db.sql
```

### Rebuild .NET services

```bash
docker-compose up -d --build policy-service query-gateway
```

## Advanced Usage

### Add Custom dbt Models

1. Create SQL file in `dbt/models/`
2. Update `dbt/models/schema.yml`
3. Restart Airflow scheduler
4. Run DAG

### Custom Data Quality Checks

Edit `airflow/dags/data_governance_dag.py` and add checks to `run_data_quality_checks` function.

### Integrate with Real DataHub

Update the `emit_lineage_to_datahub` function in the DAG to use DataHub Python SDK:

```python
from datahub.emitter.rest_emitter import DatahubRestEmitter
emitter = DatahubRestEmitter('http://datahub-gms:8080')
# emit metadata and lineage
```

## Architecture Decisions

### Why .NET 8 for Services?
- High performance for data processing
- Strong typing and compile-time safety
- Excellent PostgreSQL support via EF Core
- Native async/await for I/O operations

### Why Trino?
- Distributed query engine for large datasets
- SQL interface to multiple data sources
- Production-ready at scale
- Used by Meta, Uber, LinkedIn

### Why DataHub?
- Open-source metadata platform
- Rich lineage visualization
- Extensible via APIs
- Active community and development

## Production Considerations

### Security
- [ ] Use HTTPS/TLS for all services
- [ ] Implement proper authentication (OAuth2/OIDC)
- [ ] Rotate database credentials
- [ ] Enable Trino authentication
- [ ] Use secrets management (HashiCorp Vault)

### Scalability
- [ ] Use external PostgreSQL (RDS, Cloud SQL)
- [ ] Deploy Trino cluster (multiple workers)
- [ ] Scale Airflow with Celery executor
- [ ] Use object storage (S3, GCS) instead of MinIO
- [ ] Implement caching layer (Redis)

### Monitoring
- [ ] Add Prometheus metrics
- [ ] Setup Grafana dashboards
- [ ] Configure alerts
- [ ] Enable distributed tracing
- [ ] Log aggregation (ELK stack)

### High Availability
- [ ] Multi-instance .NET services with load balancer
- [ ] PostgreSQL replication
- [ ] Kafka for event streaming
- [ ] DataHub HA configuration

## Next Steps

1. **Extend Policies**: Add column-level encryption, row-level security
2. **Advanced Lineage**: Track transformations through dbt
3. **Real-time Quality**: Stream quality checks via Kafka
4. **ML Integration**: Use policies for feature store access
5. **Compliance Reports**: Generate GDPR/CCPA compliance reports

## Support

For issues or questions:
- Check logs: `docker-compose logs -f [service-name]`
- Review Swagger docs: http://localhost:5001/swagger
- Inspect database: `docker exec -it postgres psql -U datauser -d datagovernance`

## License

This is a demonstration system for educational purposes.
