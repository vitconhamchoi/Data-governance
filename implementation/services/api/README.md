# API Service

## Overview

REST and GraphQL API service for the Data Governance platform, providing programmatic access to metadata, quality metrics, lineage, and governance operations.

## Architecture

```
┌─────────────────────────────────────────────────┐
│            API Gateway Layer                    │
│  • Authentication & Authorization               │
│  • Rate Limiting                                │
│  • Request Routing                              │
│  • API Versioning                               │
└─────────────────────┬───────────────────────────┘
                      │
      ┌───────────────┴───────────────┐
      │                               │
┌─────▼─────────┐            ┌────────▼────────┐
│   REST API    │            │   GraphQL API   │
│  (FastAPI)    │            │   (Strawberry)  │
└─────┬─────────┘            └────────┬────────┘
      │                               │
      └───────────────┬───────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│            Service Layer                        │
│  • Metadata Service                             │
│  • Quality Service                              │
│  • Lineage Service                              │
│  • Governance Service                           │
│  • Search Service                               │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│            Data Layer                           │
│  • PostgreSQL (metadata)                        │
│  • Redis (cache)                                │
│  • Elasticsearch (search)                       │
│  • Integration with DataHub/Marquez             │
└─────────────────────────────────────────────────┘
```

## Technology Stack

- **Framework**: FastAPI (Python 3.10+)
- **GraphQL**: Strawberry GraphQL
- **ORM**: SQLAlchemy 2.0
- **Validation**: Pydantic v2
- **Authentication**: OAuth2 + JWT
- **Documentation**: OpenAPI/Swagger + GraphQL Playground

## Installation

```bash
cd implementation/services/api
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

## Configuration

```yaml
# config.yaml
server:
  host: 0.0.0.0
  port: 8000
  workers: 4
  reload: false

database:
  url: postgresql://user:pass@localhost:5432/governance
  pool_size: 20
  max_overflow: 10

redis:
  url: redis://localhost:6379/0
  cache_ttl: 3600

auth:
  jwt_secret: ${JWT_SECRET}
  jwt_algorithm: HS256
  access_token_expire_minutes: 30

integrations:
  datahub:
    url: http://datahub:8080
  marquez:
    url: http://marquez:5000
  soda:
    api_url: http://soda-cloud:5000
```

## API Endpoints

### REST API

#### Datasets

```python
# GET /api/v1/datasets
# List all datasets with pagination
GET /api/v1/datasets?page=1&size=50&zone=curated

# GET /api/v1/datasets/{dataset_id}
# Get dataset details
GET /api/v1/datasets/postgres.analytics.users

# POST /api/v1/datasets
# Register a new dataset
POST /api/v1/datasets
{
  "name": "analytics.users",
  "platform": "postgres",
  "zone": "curated",
  "owner": "data-team",
  "tags": ["pii", "gdpr"],
  "schema": {...}
}

# PUT /api/v1/datasets/{dataset_id}
# Update dataset metadata
PUT /api/v1/datasets/postgres.analytics.users

# DELETE /api/v1/datasets/{dataset_id}
# Deprecate a dataset
DELETE /api/v1/datasets/postgres.analytics.users
```

#### Data Quality

```python
# GET /api/v1/quality/datasets/{dataset_id}/metrics
# Get quality metrics for a dataset
GET /api/v1/quality/datasets/postgres.analytics.users/metrics

# GET /api/v1/quality/scans
# List quality scan results
GET /api/v1/quality/scans?dataset_id=...&status=failed

# POST /api/v1/quality/scans
# Trigger a quality scan
POST /api/v1/quality/scans
{
  "dataset_id": "postgres.analytics.users",
  "check_suite": "completeness"
}
```

#### Lineage

```python
# GET /api/v1/lineage/{dataset_id}
# Get lineage graph for a dataset
GET /api/v1/lineage/postgres.analytics.users?depth=5&direction=both

# GET /api/v1/lineage/{dataset_id}/column/{column_name}
# Get column-level lineage
GET /api/v1/lineage/postgres.analytics.users/column/email

# GET /api/v1/lineage/{dataset_id}/impact
# Get impact analysis
GET /api/v1/lineage/postgres.raw.users/impact
```

#### Governance

```python
# GET /api/v1/governance/policies
# List governance policies
GET /api/v1/governance/policies

# POST /api/v1/governance/policies
# Create a new policy
POST /api/v1/governance/policies
{
  "name": "pii-access-control",
  "description": "Restrict PII access to authorized users",
  "rules": {...}
}

# GET /api/v1/governance/access-requests
# List access requests
GET /api/v1/governance/access-requests?status=pending

# POST /api/v1/governance/access-requests
# Request dataset access
POST /api/v1/governance/access-requests
{
  "dataset_id": "postgres.analytics.users",
  "purpose": "Marketing analysis",
  "duration_days": 30
}
```

### GraphQL API

```graphql
# Schema
type Dataset {
  id: ID!
  name: String!
  platform: String!
  zone: DataZone!
  owner: User!
  tags: [Tag!]!
  schema: DatasetSchema!
  qualityMetrics: QualityMetrics
  lineage(depth: Int = 3): LineageGraph
  accessRequests: [AccessRequest!]!
  createdAt: DateTime!
  updatedAt: DateTime!
}

type QualityMetrics {
  completeness: Float!
  validity: Float!
  accuracy: Float!
  consistency: Float!
  timeliness: Float!
  lastScanAt: DateTime!
  failedChecks: [QualityCheck!]!
}

type LineageGraph {
  nodes: [LineageNode!]!
  edges: [LineageEdge!]!
}

type Query {
  # Search datasets
  datasets(
    search: String
    zone: DataZone
    tags: [String!]
    page: Int = 1
    size: Int = 50
  ): DatasetConnection!

  # Get single dataset
  dataset(id: ID!): Dataset

  # Search across all metadata
  search(query: String!, types: [EntityType!]): SearchResult!

  # Quality metrics
  qualityScans(
    datasetId: ID
    status: ScanStatus
    from: DateTime
    to: DateTime
  ): [QualityScan!]!

  # Lineage
  lineage(
    datasetId: ID!
    depth: Int = 3
    direction: LineageDirection = BOTH
  ): LineageGraph!

  # Governance
  policies: [Policy!]!
  accessRequests(status: RequestStatus): [AccessRequest!]!
}

type Mutation {
  # Register dataset
  registerDataset(input: RegisterDatasetInput!): Dataset!

  # Update dataset
  updateDataset(id: ID!, input: UpdateDatasetInput!): Dataset!

  # Trigger quality scan
  triggerQualityScan(datasetId: ID!): QualityScan!

  # Request access
  requestAccess(input: AccessRequestInput!): AccessRequest!

  # Approve/deny access
  reviewAccessRequest(
    requestId: ID!
    approved: Boolean!
    comment: String
  ): AccessRequest!
}
```

Example queries:

```graphql
# Search datasets with quality issues
query SearchDatasetsWithIssues {
  datasets(tags: ["pii"], zone: CURATED) {
    edges {
      node {
        id
        name
        owner {
          name
          email
        }
        qualityMetrics {
          completeness
          failedChecks {
            name
            severity
            message
          }
        }
      }
    }
  }
}

# Get complete lineage with column details
query GetDatasetLineage {
  lineage(datasetId: "postgres.analytics.users", depth: 5) {
    nodes {
      id
      name
      type
      ... on Dataset {
        schema {
          columns {
            name
            type
            columnLineage {
              sourceColumn
              transformation
            }
          }
        }
      }
    }
    edges {
      source
      target
      transformationType
    }
  }
}
```

## Usage Examples

### Python Client

```python
import requests

API_URL = "http://localhost:8000/api/v1"
TOKEN = "your-jwt-token"

headers = {"Authorization": f"Bearer {TOKEN}"}

# Get dataset
response = requests.get(
    f"{API_URL}/datasets/postgres.analytics.users",
    headers=headers
)
dataset = response.json()

# Trigger quality scan
response = requests.post(
    f"{API_URL}/quality/scans",
    headers=headers,
    json={"dataset_id": "postgres.analytics.users"}
)
scan_result = response.json()

# Get lineage
response = requests.get(
    f"{API_URL}/lineage/postgres.analytics.users",
    params={"depth": 5},
    headers=headers
)
lineage = response.json()
```

### GraphQL Client

```python
from gql import gql, Client
from gql.transport.requests import RequestsHTTPTransport

transport = RequestsHTTPTransport(
    url="http://localhost:8000/graphql",
    headers={"Authorization": f"Bearer {TOKEN}"}
)

client = Client(transport=transport, fetch_schema_from_transport=True)

query = gql("""
    query GetDataset($id: ID!) {
        dataset(id: $id) {
            name
            zone
            qualityMetrics {
                completeness
                validity
            }
        }
    }
""")

result = client.execute(query, variable_values={"id": "postgres.analytics.users"})
```

## Authentication & Authorization

### JWT Authentication

```python
# Login
POST /api/v1/auth/login
{
  "username": "user@example.com",
  "password": "password"
}

# Response
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "token_type": "bearer",
  "expires_in": 1800
}

# Use token in requests
GET /api/v1/datasets
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Role-Based Access Control (RBAC)

```python
# Roles
- admin: Full access
- data_steward: Manage metadata and policies
- data_analyst: Read access to datasets
- data_engineer: Read/write for pipelines
```

## Development

```bash
# Install dependencies
pip install -r requirements.txt

# Run migrations
alembic upgrade head

# Start development server
uvicorn app.main:app --reload --port 8000

# Run tests
pytest tests/ -v

# Code quality
black app/
ruff check app/
mypy app/
```

## Directory Structure

```
api/
├── app/
│   ├── __init__.py
│   ├── main.py
│   ├── config.py
│   ├── api/
│   │   ├── v1/
│   │   │   ├── endpoints/
│   │   │   │   ├── datasets.py
│   │   │   │   ├── quality.py
│   │   │   │   ├── lineage.py
│   │   │   │   └── governance.py
│   │   │   └── router.py
│   │   └── graphql/
│   │       ├── schema.py
│   │       ├── resolvers/
│   │       └── types/
│   ├── services/
│   │   ├── metadata_service.py
│   │   ├── quality_service.py
│   │   ├── lineage_service.py
│   │   └── governance_service.py
│   ├── models/
│   ├── schemas/
│   ├── db/
│   └── auth/
├── tests/
├── alembic/
├── requirements.txt
├── Dockerfile
└── README.md
```

## Deployment

### Docker

```bash
docker build -t governance-api .
docker run -p 8000:8000 governance-api
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: governance-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: governance-api
  template:
    metadata:
      labels:
        app: governance-api
    spec:
      containers:
      - name: api
        image: governance-api:latest
        ports:
        - containerPort: 8000
        env:
        - name: DATABASE_URL
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: url
```

## Monitoring

- Health check: `GET /health`
- Metrics: `GET /metrics` (Prometheus format)
- API docs: `GET /docs` (Swagger UI)
- GraphQL playground: `GET /graphql`

## References

- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Strawberry GraphQL](https://strawberry.rocks/)
