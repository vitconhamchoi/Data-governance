# Data Governance System - Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                            CLIENT APPLICATIONS                           │
│                    (BI Tools, Analytics, Applications)                   │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 │ SQL Query + User Role
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        QUERY GATEWAY (.NET 8)                            │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  1. Receive query + role                                        │   │
│  │  2. Extract dataset name                                        │   │
│  │  3. Fetch policies from Policy Service                         │   │
│  │  4. Execute query via Trino                                     │   │
│  │  5. Apply masking/redaction based on policies                   │   │
│  │  6. Return masked results                                       │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                           │
│  Services:                                                               │
│  - PolicyService (HTTP Client)                                          │
│  - TrinoService (Query Execution)                                       │
│  - MaskingService (Data Protection)                                     │
└──────────────┬──────────────────────────────┬───────────────────────────┘
               │                              │
               │ HTTP                         │ SQL
               │                              │
               ▼                              ▼
┌──────────────────────────────┐   ┌──────────────────────────────────────┐
│   POLICY SERVICE (.NET 8)    │   │          TRINO                       │
│                              │   │   Distributed Query Engine           │
│  ┌────────────────────────┐  │   │                                      │
│  │  Policy Management     │  │   │  - Connect to PostgreSQL            │
│  │  - CRUD Operations     │  │   │  - Execute SQL queries              │
│  │  - Entity Framework    │  │   │  - Return raw results               │
│  │  - PostgreSQL Storage  │  │   │                                      │
│  └────────────────────────┘  │   └────┬─────────────────────────────────┘
│                              │        │
│  Entities:                   │        │
│  - Id, Dataset, Column       │        │
│  - Rule (mask/deny)          │        │
│  - Role, CreatedAt           │        │
└──────────────┬───────────────┘        │
               │                        │
               │                        │
               ▼                        ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           POSTGRESQL                                     │
│  ┌───────────────────┬────────────────────┬──────────────────────────┐  │
│  │   policies        │    users (PII)     │    orders                │  │
│  │   - governance    │    - email         │    - transactions        │  │
│  │     rules         │    - phone         │    - amounts             │  │
│  └───────────────────┴────────────────────┴──────────────────────────┘  │
└────────────────┬────────────────────────────────────────────────────────┘
                 │
                 │ Metadata
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        DATA ORCHESTRATION                                │
│                                                                           │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      APACHE AIRFLOW                             │   │
│  │  ┌───────────────────────────────────────────────────────────┐  │   │
│  │  │  DAG: data_governance_pipeline                            │  │   │
│  │  │                                                            │  │   │
│  │  │  1. run_dbt_models                                        │  │   │
│  │  │     └─> Transform data (users_with_orders)               │  │   │
│  │  │                                                            │  │   │
│  │  │  2. run_quality_checks (Soda Core)                       │  │   │
│  │  │     ├─> Users: row_count > 0                             │  │   │
│  │  │     ├─> Users: email NOT NULL                            │  │   │
│  │  │     ├─> Orders: row_count > 0                            │  │   │
│  │  │     └─> Orders: user_id NOT NULL                         │  │   │
│  │  │                                                            │  │   │
│  │  │  3. emit_lineage_to_datahub                              │  │   │
│  │  │     └─> Send metadata to DataHub                         │  │   │
│  │  └───────────────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                           │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                            dbt                                   │   │
│  │  Models:                                                         │   │
│  │  - users_with_orders.sql (aggregations)                         │   │
│  │  - Schema with PII tags                                         │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │
                                     │ Lineage
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           DATAHUB OSS                                    │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  Metadata Catalog                                               │   │
│  │  - Dataset discovery                                            │   │
│  │  - Column-level metadata                                        │   │
│  │  - PII/Sensitive data tags                                      │   │
│  │  - Data lineage visualization                                   │   │
│  │  - Search & browse                                              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                           │
│  Components:                                                             │
│  - datahub-gms (Metadata service)                                       │
│  - datahub-frontend (UI)                                                │
│  - Elasticsearch (Search)                                               │
│  - MySQL (Storage)                                                      │
└─────────────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                        SUPPORTING SERVICES                               │
│                                                                           │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐                    │
│  │   MinIO     │  │   Kafka     │  │  Zookeeper   │                    │
│  │   Object    │  │   Message   │  │  Kafka       │                    │
│  │   Storage   │  │   Bus       │  │  Coord       │                    │
│  └─────────────┘  └─────────────┘  └──────────────┘                    │
└─────────────────────────────────────────────────────────────────────────┘
```

## Data Flow

### 1. Data Ingestion & Quality
```
CSV Files → Airflow → PostgreSQL → Quality Checks → DataHub (lineage)
```

### 2. Query Execution with Policy Enforcement
```
Client → Query Gateway → Policy Service (get rules)
                      ↓
                   Trino (execute SQL)
                      ↓
                   Apply masking
                      ↓
                   Return masked data
```

### 3. Metadata Flow
```
dbt models → DataHub GMS → Elasticsearch → DataHub UI
```

## Component Responsibilities

### PolicyService (.NET 8)
- **Purpose**: Centralized policy management
- **Storage**: PostgreSQL with EF Core
- **API**: RESTful CRUD operations
- **Schema**:
  ```
  Policy {
    Id, Dataset, Column, Rule, Role, CreatedAt
  }
  ```

### QueryGateway (.NET 8)
- **Purpose**: Query proxy with policy enforcement
- **Integration**: Trino + PolicyService
- **Masking**:
  - Email: `john@example.com` → `j***@example.com`
  - Phone: `+1-555-0101` → `****0101`
  - Deny: Any value → `[REDACTED]`

### Trino
- **Purpose**: Distributed SQL query engine
- **Connectors**: PostgreSQL catalog
- **Usage**: Execute queries across data sources

### DataHub
- **Purpose**: Metadata catalog and lineage
- **Features**:
  - Dataset discovery
  - Column-level metadata
  - PII tagging
  - Lineage visualization

### Airflow + dbt
- **Purpose**: Data pipeline orchestration
- **Workflow**:
  1. Transform data (dbt)
  2. Quality checks (Soda Core)
  3. Emit lineage (DataHub)

## Security & Governance

### PII Protection
- **Identification**: Email, phone columns tagged in dbt schema
- **Enforcement**: Query Gateway applies masking
- **Policies**: Stored in PostgreSQL, retrieved per query

### Role-Based Access
- **Admin**: Full access, no masking
- **Analyst**: Masked PII (email, phone)
- **Viewer**: Denied fields show [REDACTED]

### Data Quality
- **Checks**: Row count, null validation
- **Enforcement**: Pipeline fails if checks fail
- **Tracking**: Results stored in database

### Lineage
- **Source**: CSV files
- **Transformations**: dbt models
- **Target**: PostgreSQL tables
- **Visualization**: DataHub UI

## Scalability Considerations

### Current Setup (Local)
- Single PostgreSQL instance
- Single Trino coordinator
- Local Airflow executor
- Embedded DataHub

### Production Recommendations
- PostgreSQL: Master-replica setup
- Trino: Multi-worker cluster
- Airflow: Celery executor with workers
- DataHub: Kubernetes deployment
- .NET Services: Load balanced instances

## Port Mapping

| Service | Port | Purpose |
|---------|------|---------|
| PostgreSQL | 5432 | Database |
| Policy Service | 5001 | Policy API |
| Query Gateway | 5002 | Query API |
| Trino | 8080 | Query engine |
| Airflow | 8081 | Pipeline UI |
| DataHub GMS | 8091 | Metadata API |
| MinIO | 9000/9001 | Object storage |
| DataHub UI | 9002 | Metadata UI |
| Elasticsearch | 9200 | Search |

## Technology Stack

- **Backend**: .NET 8 (C#)
- **Database**: PostgreSQL 15
- **ORM**: Entity Framework Core 8
- **Query Engine**: Trino (latest)
- **Orchestration**: Apache Airflow 2.11.1
- **Transformation**: dbt
- **Metadata**: DataHub OSS
- **Storage**: MinIO
- **Message Bus**: Kafka 7.4
- **Containerization**: Docker + Docker Compose
