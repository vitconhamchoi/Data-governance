# Lineage Tracking - OpenLineage Integration

## Overview

This module implements end-to-end data lineage tracking using OpenLineage, an open standard for metadata and lineage collection.

## Architecture

```
┌─────────────────────────────────────────────────┐
│         Data Sources & Processes                │
├─────────────────────────────────────────────────┤
│  • Airflow DAGs                                 │
│  • Spark Jobs                                   │
│  • dbt Models                                   │
│  • Custom ETL Scripts                           │
└─────────────────────┬───────────────────────────┘
                      │ OpenLineage Events
                      ↓
┌─────────────────────────────────────────────────┐
│         Marquez (Lineage Backend)               │
├─────────────────────────────────────────────────┤
│  • Lineage Graph Storage                        │
│  • Dataset Versioning                           │
│  • Job Run History                              │
│  • API & Web UI                                 │
└─────────────────────┬───────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────┐
│         Lineage Visualization & Analysis        │
├─────────────────────────────────────────────────┤
│  • Column-level Lineage                         │
│  • Impact Analysis                              │
│  • Root Cause Analysis                          │
│  • Compliance Reporting                         │
└─────────────────────────────────────────────────┘
```

## Features

- **End-to-End Lineage**: Track data from source to consumption
- **Column-Level Lineage**: Field-level transformation tracking
- **Cross-Platform**: Works with Airflow, Spark, dbt, etc.
- **Real-Time Updates**: Lineage updated as jobs run
- **Impact Analysis**: Understand downstream dependencies
- **Time Travel**: View lineage at any point in time

## Installation

### Marquez Backend

```bash
# Docker Compose
cd implementation/integrations/lineage-tracking
docker-compose up -d

# Kubernetes
helm repo add marquez https://marquezproject.github.io/marquez/helm
helm install marquez marquez/marquez
```

### OpenLineage Clients

```bash
# Python client
pip install openlineage-python openlineage-airflow openlineage-spark

# Java client (for Spark)
# Add to spark-submit or spark-defaults.conf
spark.jars.packages io.openlineage:openlineage-spark_2.12:0.28.0
```

## Configuration

### Marquez Configuration

```yaml
# marquez.yml
server:
  applicationConnectors:
    - type: http
      port: 5000

database:
  driverClass: org.postgresql.Driver
  url: jdbc:postgresql://localhost:5432/marquez
  user: ${POSTGRES_USER}
  password: ${POSTGRES_PASSWORD}
```

### Airflow Integration

```python
# airflow.cfg or environment variables
[openlineage]
transport = {"type": "http", "url": "http://marquez:5000"}
namespace = production
```

### Spark Integration

```bash
# spark-defaults.conf
spark.openlineage.transport.type=http
spark.openlineage.transport.url=http://marquez:5000
spark.openlineage.namespace=production
spark.extraListeners=io.openlineage.spark.agent.OpenLineageSparkListener
```

## Usage

### Airflow DAG with Lineage

```python
from airflow import DAG
from airflow.providers.postgres.operators.postgres import PostgresOperator
from datetime import datetime

with DAG(
    'user_analytics',
    start_date=datetime(2024, 1, 1),
    schedule_interval='@daily',
    catchup=False
) as dag:

    # Lineage automatically captured
    extract_users = PostgresOperator(
        task_id='extract_users',
        postgres_conn_id='source_db',
        sql='''
            SELECT user_id, email, created_at
            FROM users
            WHERE created_at >= '{{ ds }}'
        ''',
        # Output dataset automatically tracked
        outlets=[{"namespace": "postgres", "name": "staging.users"}]
    )

    transform_users = PostgresOperator(
        task_id='transform_users',
        postgres_conn_id='warehouse_db',
        sql='''
            INSERT INTO analytics.user_daily_stats
            SELECT
                DATE(created_at) as date,
                COUNT(*) as new_users,
                COUNT(DISTINCT email) as unique_emails
            FROM staging.users
            GROUP BY DATE(created_at)
        ''',
        inlets=[{"namespace": "postgres", "name": "staging.users"}],
        outlets=[{"namespace": "postgres", "name": "analytics.user_daily_stats"}]
    )

    extract_users >> transform_users
```

### Spark Job with Lineage

```python
from pyspark.sql import SparkSession

spark = SparkSession.builder \
    .appName("user-transformation") \
    .config("spark.openlineage.transport.type", "http") \
    .config("spark.openlineage.transport.url", "http://marquez:5000") \
    .config("spark.openlineage.namespace", "production") \
    .getOrCreate()

# Read data - lineage automatically captured
df_users = spark.read \
    .format("parquet") \
    .load("s3://data-lake/raw/users/")

# Transform
df_transformed = df_users \
    .filter("status = 'active'") \
    .groupBy("country") \
    .count()

# Write - lineage automatically captured
df_transformed.write \
    .format("parquet") \
    .mode("overwrite") \
    .save("s3://data-lake/curated/user_stats_by_country/")
```

### dbt with Lineage

```yaml
# profiles.yml
production:
  target: prod
  outputs:
    prod:
      type: postgres
      host: localhost
      port: 5432
      database: warehouse
      schema: analytics

      # OpenLineage configuration
      openlineage:
        namespace: production
        transport:
          type: http
          url: http://marquez:5000
```

```sql
-- models/user_stats.sql
-- Lineage automatically tracked from ref() and source()
{{ config(materialized='table') }}

SELECT
    u.user_id,
    u.email,
    COUNT(o.order_id) as total_orders,
    SUM(o.amount) as total_spent
FROM {{ source('raw', 'users') }} u
LEFT JOIN {{ source('raw', 'orders') }} o
    ON u.user_id = o.user_id
GROUP BY u.user_id, u.email
```

### Manual Event Emission

```python
from openlineage.client import OpenLineageClient
from openlineage.client.run import RunEvent, RunState, Run, Job
from openlineage.client.facet import SqlJobFacet, SchemaDatasetFacet, SchemaField

client = OpenLineageClient(url="http://marquez:5000")

# Define job
job = Job(namespace="custom_etl", name="data_pipeline_v1")

# Define run
run = Run(runId="unique-run-id-123")

# Define input datasets
input_dataset = {
    "namespace": "postgres",
    "name": "source.users",
    "facets": {
        "schema": SchemaDatasetFacet(
            fields=[
                SchemaField(name="user_id", type="INTEGER"),
                SchemaField(name="email", type="VARCHAR"),
                SchemaField(name="created_at", type="TIMESTAMP")
            ]
        )
    }
}

# Define output datasets
output_dataset = {
    "namespace": "postgres",
    "name": "analytics.user_stats",
    "facets": {
        "schema": SchemaDatasetFacet(
            fields=[
                SchemaField(name="date", type="DATE"),
                SchemaField(name="user_count", type="INTEGER")
            ]
        )
    }
}

# Emit START event
client.emit(
    RunEvent(
        eventType=RunState.START,
        eventTime="2024-01-01T10:00:00Z",
        run=run,
        job=job,
        inputs=[input_dataset],
        outputs=[output_dataset],
        producer="custom_etl/1.0.0"
    )
)

# ... run your ETL logic ...

# Emit COMPLETE event
client.emit(
    RunEvent(
        eventType=RunState.COMPLETE,
        eventTime="2024-01-01T10:30:00Z",
        run=run,
        job=job,
        inputs=[input_dataset],
        outputs=[output_dataset],
        producer="custom_etl/1.0.0"
    )
)
```

### Query Lineage via API

```python
import requests

MARQUEZ_URL = "http://localhost:5000"

# Get dataset lineage
dataset_namespace = "postgres"
dataset_name = "analytics.user_stats"

response = requests.get(
    f"{MARQUEZ_URL}/api/v1/lineage",
    params={
        "nodeId": f"{dataset_namespace}/{dataset_name}",
        "depth": 5
    }
)

lineage_graph = response.json()

# Get job runs
job_namespace = "airflow"
job_name = "user_analytics"

response = requests.get(
    f"{MARQUEZ_URL}/api/v1/namespaces/{job_namespace}/jobs/{job_name}/runs"
)

job_runs = response.json()
```

## Column-Level Lineage

```python
from openlineage.client.facet import ColumnLineageDatasetFacet, Fields

# Define column-level lineage
column_lineage = ColumnLineageDatasetFacet(
    fields={
        "total_spent": Fields(
            inputFields=[
                {"namespace": "postgres", "name": "raw.orders", "field": "amount"}
            ],
            transformationType="SUM",
            transformationDescription="SUM(o.amount)"
        ),
        "total_orders": Fields(
            inputFields=[
                {"namespace": "postgres", "name": "raw.orders", "field": "order_id"}
            ],
            transformationType="COUNT",
            transformationDescription="COUNT(o.order_id)"
        )
    }
)

# Add to output dataset facets
output_dataset["facets"]["columnLineage"] = column_lineage
```

## Impact Analysis

```python
def analyze_impact(dataset_name):
    """Find all downstream dependencies of a dataset"""
    response = requests.get(
        f"{MARQUEZ_URL}/api/v1/lineage",
        params={
            "nodeId": dataset_name,
            "depth": 10,
            "withDownstream": True
        }
    )

    lineage = response.json()

    # Extract all downstream datasets
    downstream = []
    for node in lineage.get("graph", []):
        if node["type"] == "DATASET" and node["id"] != dataset_name:
            downstream.append(node["data"]["name"])

    return downstream

# Example: What will be affected if we change users table?
affected = analyze_impact("postgres/raw.users")
print(f"Changing raw.users will affect {len(affected)} datasets:")
for dataset in affected:
    print(f"  - {dataset}")
```

## Directory Structure

```
lineage-tracking/
├── config/
│   ├── docker-compose.yml
│   ├── marquez.yml
│   └── integrations/
│       ├── airflow.cfg
│       ├── spark-defaults.conf
│       └── dbt-profiles.yml
├── clients/
│   ├── airflow_lineage.py
│   ├── spark_lineage.py
│   └── custom_emitter.py
├── scripts/
│   ├── setup_marquez.sh
│   ├── backfill_lineage.py
│   └── export_lineage.py
├── dashboards/
│   └── lineage_grafana.json
└── README.md
```

## Integration with Data Platform

- **Metadata Store**: Lineage events sent to both Marquez and DataHub
- **Data Quality**: Link quality check failures to lineage
- **Pipeline Orchestration**: Automatic lineage from Airflow/Dagster
- **Monitoring**: Lineage metrics in observability stack

## Monitoring

- Track lineage event volume
- Monitor lineage API latency
- Alert on missing lineage for critical datasets

## Security

- API authentication with tokens
- Column-level lineage for PII tracking
- Audit log of lineage access

## References

- [OpenLineage Specification](https://openlineage.io/)
- [Marquez Documentation](https://marquezproject.github.io/marquez/)
- [OpenLineage Integrations](https://openlineage.io/docs/integrations/about)
