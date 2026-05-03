# Metadata Store - DataHub Integration

## Overview

This module provides integration with DataHub, a modern metadata platform that enables data discovery, collaboration, and governance at scale.

## Architecture

```
┌─────────────────────────────────────────────────┐
│           DataHub Metadata Store                │
├─────────────────────────────────────────────────┤
│  • Metadata Ingestion Pipeline                  │
│  • Graph Database (Neo4j)                       │
│  • Search Index (Elasticsearch)                 │
│  • Web UI & GraphQL API                         │
└─────────────────────────────────────────────────┘
                    ↑
                    │
┌─────────────────────────────────────────────────┐
│         Ingestion Connectors                    │
├─────────────────────────────────────────────────┤
│  • Database Connectors (PostgreSQL, MySQL)      │
│  • Data Lake Connectors (S3, ADLS)             │
│  • BI Tool Connectors (Tableau, Looker)        │
│  • ETL Tool Connectors (Airflow, dbt)          │
│  • Custom Connectors                           │
└─────────────────────────────────────────────────┘
```

## Features

- **Data Discovery**: Search and browse datasets, schemas, and columns
- **Lineage Tracking**: Visualize data flow across systems
- **Data Governance**: Tag datasets with governance metadata
- **Access Control**: Manage permissions and ownership
- **Impact Analysis**: Understand downstream dependencies

## Installation

### Docker Compose (Development)

```bash
cd implementation/integrations/metadata-store
docker-compose up -d
```

### Kubernetes (Production)

```bash
helm repo add datahub https://helm.datahubproject.io/
helm install datahub datahub/datahub
```

## Configuration

### Environment Variables

```bash
DATAHUB_GMS_HOST=localhost
DATAHUB_GMS_PORT=8080
DATAHUB_KAFKA_BOOTSTRAP_SERVERS=localhost:9092
NEO4J_HOST=localhost
NEO4J_PORT=7687
ELASTICSEARCH_HOST=localhost
ELASTICSEARCH_PORT=9200
```

### config.yml

```yaml
source:
  type: postgres
  config:
    host_port: localhost:5432
    database: mydb
    username: ${POSTGRES_USER}
    password: ${POSTGRES_PASSWORD}

sink:
  type: datahub-rest
  config:
    server: http://localhost:8080
```

## Usage

### Ingest Metadata from PostgreSQL

```python
from datahub.ingestion.run.pipeline import Pipeline

pipeline = Pipeline.create({
    "source": {
        "type": "postgres",
        "config": {
            "host_port": "localhost:5432",
            "database": "mydb",
        }
    },
    "sink": {
        "type": "datahub-rest",
        "config": {
            "server": "http://localhost:8080"
        }
    }
})

pipeline.run()
```

### Query Metadata via API

```python
from datahub.emitter.rest_emitter import DatahubRestEmitter
from datahub.metadata.schema_classes import DatasetPropertiesClass

emitter = DatahubRestEmitter("http://localhost:8080")

# Get dataset metadata
dataset_urn = "urn:li:dataset:(urn:li:dataPlatform:postgres,mydb.public.users,PROD)"
metadata = emitter.get_aspect(dataset_urn, DatasetPropertiesClass)
```

### Add Custom Tags

```python
from datahub.emitter.mce_builder import make_tag_urn
from datahub.emitter.mcp import MetadataChangeProposalWrapper
from datahub.metadata.schema_classes import GlobalTagsClass, TagAssociationClass

tags = GlobalTagsClass(
    tags=[
        TagAssociationClass(tag=make_tag_urn("PII")),
        TagAssociationClass(tag=make_tag_urn("GDPR"))
    ]
)

proposal = MetadataChangeProposalWrapper(
    entityUrn=dataset_urn,
    aspect=tags
)

emitter.emit_mcp(proposal)
```

## Directory Structure

```
metadata-store/
├── config/
│   ├── docker-compose.yml
│   ├── ingestion/
│   │   ├── postgres.yml
│   │   ├── s3.yml
│   │   └── airflow.yml
│   └── kubernetes/
│       └── values.yaml
├── connectors/
│   ├── custom_source.py
│   └── custom_transformer.py
├── scripts/
│   ├── setup.sh
│   ├── ingest.sh
│   └── backup.sh
└── README.md
```

## Integration with Data Platform

This metadata store integrates with:
- **Data Lakehouse**: Tracks all datasets across Raw/Standardized/Curated/Trusted zones
- **ETL Pipelines**: Captures lineage from Airflow/dbt runs
- **BI Tools**: Links dashboards to source datasets
- **Data Quality**: Displays quality metrics alongside metadata

## Monitoring

DataHub exposes Prometheus metrics at `/metrics`:
- Ingestion pipeline status
- API request latency
- Search query performance
- Graph database health

## Security

- **Authentication**: OIDC/OAuth2 integration
- **Authorization**: Role-based access control (RBAC)
- **Audit Logging**: All metadata changes logged
- **Encryption**: TLS for data in transit

## Troubleshooting

See [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) for common issues.

## References

- [DataHub Documentation](https://datahubproject.io/docs)
- [DataHub GitHub](https://github.com/datahub-project/datahub)
