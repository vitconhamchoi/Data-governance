# Data Governance System

Production-ready Data Governance platform with real implementation, runnable locally via Docker Compose.

## Features

- ✅ **Metadata Catalog**: DataHub OSS for metadata management
- ✅ **Data Pipeline**: Apache Airflow + dbt for ETL/ELT
- ✅ **Query Engine**: Trino for distributed SQL queries
- ✅ **Data Quality**: Soda Core integration for automated checks
- ✅ **Policy Enforcement**: .NET 8 services for RBAC and masking
- ✅ **Sample Data**: Real datasets (users, orders) with PII marking

## Architecture

```
CSV → Airflow → PostgreSQL → Trino → Query Gateway → Masked Results
                     ↓              ↓
                 DataHub      Policy Service
```

## Quick Start

```bash
cd dotnet2
chmod +x scripts/setup.sh
./scripts/setup.sh

# Wait for services to start, then run demo
chmod +x scripts/demo.sh
./scripts/demo.sh
```

## Components

### 1. PolicyService (.NET 8)
ASP.NET Core Web API for policy management:
- CRUD operations for policies
- Stores: dataset, column, rule (mask/deny), role
- PostgreSQL storage with Entity Framework Core

### 2. QueryGateway (.NET 8)
ASP.NET Core service acting as proxy:
- Accepts SQL query + user role
- Fetches policies from PolicyService
- Executes via Trino
- Applies masking based on policies

### 3. Data Pipeline
- **Airflow**: Orchestrates ETL jobs
- **dbt**: Data transformations
- **Soda Core**: Data quality checks

### 4. Metadata & Storage
- **DataHub**: Metadata catalog and lineage
- **PostgreSQL**: Primary database
- **MinIO**: Object storage
- **Trino**: Query engine

## Demo Scenario

1. **Load sample data** ✓ (users, orders tables)
2. **Run pipeline** → Airflow DAG with quality checks
3. **View lineage** → DataHub UI
4. **Create policy** → Analyst cannot see full email/phone
5. **Query comparison**:
   - Admin → Full data
   - Analyst → Masked PII

## Access URLs

- Airflow: http://localhost:8081 (admin/admin)
- DataHub: http://localhost:9002
- Policy API: http://localhost:5001/swagger
- Query Gateway: http://localhost:5002/swagger
- MinIO: http://localhost:9001 (minioadmin/minioadmin123)
- Trino: http://localhost:8080

## API Examples

### Create Policy
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

### Query with Masking
```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT * FROM postgresql.public.users LIMIT 3",
    "role": "analyst",
    "dataset": "users"
  }'
```

## Tech Stack

- **Backend**: .NET 8 (ASP.NET Core)
- **Database**: PostgreSQL 15
- **Query Engine**: Trino
- **Orchestration**: Apache Airflow 2.11.1
- **Transformations**: dbt
- **Metadata**: DataHub OSS
- **Storage**: MinIO
- **Data Quality**: Soda Core (Python)

## Documentation

- **[RUN_GUIDE.md](RUN_GUIDE.md)**: Complete step-by-step guide
- **[API Documentation](http://localhost:5001/swagger)**: Interactive API docs
- Swagger UI for both services

## Requirements

- Docker 20.10+
- Docker Compose 2.0+
- 8GB RAM minimum (16GB recommended)

## Project Structure

```
dotnet2/
├── docker-compose.yml          # All services orchestration
├── services/
│   ├── PolicyService/          # .NET 8 Policy API
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Data/
│   │   └── Dockerfile
│   └── QueryGateway/           # .NET 8 Query Gateway
│       ├── Controllers/
│       ├── Services/
│       ├── Models/
│       └── Dockerfile
├── airflow/
│   └── dags/                   # Airflow pipelines
│       └── data_governance_dag.py
├── dbt/
│   ├── models/                 # dbt transformations
│   └── dbt_project.yml
├── data/                       # Sample CSV files
├── scripts/                    # Setup and demo scripts
├── trino/
│   └── catalog/                # Trino connectors
└── RUN_GUIDE.md                # Complete documentation
```

## Key Features

### Policy Enforcement
- **Masking**: Email (j***@domain.com), Phone (****1234)
- **Deny**: Complete redaction [REDACTED]
- **Role-based**: Different rules per role

### Data Quality
Automated checks:
- Row count validation
- Null value detection
- Pipeline failure on check failure

### Data Lineage
- CSV → PostgreSQL tracking
- dbt transformation lineage
- DataHub visualization

## Production Readiness

✅ Real integration (Trino + DataHub)
✅ No mock data
✅ No pseudo code
✅ Full source code (C#)
✅ Docker setup
✅ Step-by-step run guide
✅ API examples (curl)
✅ Runnable locally

## Sample Output

**Admin Query:**
```json
{
  "data": [{"email": "john.doe@example.com", "phone": "+1-555-0101"}],
  "appliedPolicies": []
}
```

**Analyst Query:**
```json
{
  "data": [{"email": "j***@example.com", "phone": "****0101"}],
  "appliedPolicies": ["email:mask", "phone:mask"]
}
```

## Next Steps

1. Run `./scripts/setup.sh` to start all services
2. Run `./scripts/demo.sh` to see the system in action
3. Check `RUN_GUIDE.md` for detailed documentation
4. Explore Swagger UI at http://localhost:5001/swagger

## License

Educational demonstration system.
