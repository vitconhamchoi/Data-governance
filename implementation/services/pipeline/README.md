# Data Pipeline Service

## Overview

Scalable data processing pipelines for ingestion, transformation, and quality enforcement across the lakehouse zones (Raw → Standardized → Curated → Trusted).

## Architecture

```
Data Sources → Ingestion → Processing → Storage
     │            │            │          │
     │            ↓            ↓          ↓
  External    [Kafka]   [Spark/Flink]  [Lakehouse]
  Systems       ↓            ↓            ↓
             Events    Transformations  Zones
                           ↓
                      Quality Gates
```

## Technology Stack

- **Orchestration**: Apache Airflow
- **Stream Processing**: Apache Spark Structured Streaming / Apache Flink
- **Batch Processing**: Apache Spark
- **Messaging**: Apache Kafka
- **Storage**: Delta Lake / Apache Iceberg

## Pipeline Types

### 1. Ingestion Pipelines

Ingest data from various sources into the Raw zone.

```python
# pipelines/ingestion/postgres_to_raw.py
from pyspark.sql import SparkSession

def ingest_postgres_table(table_name, jdbc_url):
    spark = SparkSession.builder.getOrCreate()

    df = spark.read \
        .format("jdbc") \
        .option("url", jdbc_url) \
        .option("dbtable", table_name) \
        .option("user", POSTGRES_USER) \
        .option("password", POSTGRES_PASSWORD) \
        .load()

    # Write to Raw zone (immutable, append-only)
    df.write \
        .format("delta") \
        .mode("append") \
        .partitionBy("ingestion_date") \
        .save(f"s3://lakehouse/raw/{table_name}/")
```

### 2. Transformation Pipelines

Transform data between lakehouse zones with quality gates.

```python
# pipelines/transformation/raw_to_standardized.py
from pyspark.sql import functions as F
from pyspark.sql.types import *

def standardize_users(spark):
    # Read from Raw zone
    df_raw = spark.read.format("delta").load("s3://lakehouse/raw/users/")

    # Standardization: schema, types, deduplication
    df_standardized = df_raw \
        .withColumn("user_id", F.col("user_id").cast(LongType())) \
        .withColumn("email", F.lower(F.trim(F.col("email")))) \
        .withColumn("created_at", F.col("created_at").cast(TimestampType())) \
        .dropDuplicates(["user_id"]) \
        .withColumn("standardized_at", F.current_timestamp())

    # Write to Standardized zone
    df_standardized.write \
        .format("delta") \
        .mode("overwrite") \
        .save("s3://lakehouse/standardized/users/")
```

### 3. Stream Processing Pipelines

Real-time data processing from Kafka to Lakehouse.

```python
# pipelines/streaming/telemetry_stream.py
from pyspark.sql import SparkSession
from pyspark.sql import functions as F

spark = SparkSession.builder \
    .appName("telemetry-stream-processor") \
    .getOrCreate()

# Read from Kafka
df_stream = spark.readStream \
    .format("kafka") \
    .option("kafka.bootstrap.servers", "kafka:9092") \
    .option("subscribe", "telemetry.raw") \
    .load()

# Parse and transform
df_parsed = df_stream \
    .select(F.from_json(F.col("value").cast("string"), schema).alias("data")) \
    .select("data.*") \
    .withColumn("processing_time", F.current_timestamp())

# Write to Delta Lake with streaming
query = df_parsed.writeStream \
    .format("delta") \
    .outputMode("append") \
    .option("checkpointLocation", "s3://checkpoints/telemetry/") \
    .partitionBy("date") \
    .start("s3://lakehouse/raw/telemetry/")

query.awaitTermination()
```

## Airflow DAGs

### Daily Batch Pipeline

```python
# dags/daily_data_pipeline.py
from airflow import DAG
from airflow.providers.apache.spark.operators.spark_submit import SparkSubmitOperator
from airflow.operators.python import PythonOperator
from datetime import datetime, timedelta

default_args = {
    'owner': 'data-team',
    'depends_on_past': False,
    'retries': 2,
    'retry_delay': timedelta(minutes=5)
}

with DAG(
    'daily_data_pipeline',
    default_args=default_args,
    schedule_interval='@daily',
    start_date=datetime(2024, 1, 1),
    catchup=False
) as dag:

    # Ingest from sources
    ingest_users = SparkSubmitOperator(
        task_id='ingest_users',
        application='pipelines/ingestion/postgres_to_raw.py',
        conf={'spark.executor.memory': '4g'}
    )

    # Standardize
    standardize = SparkSubmitOperator(
        task_id='standardize_users',
        application='pipelines/transformation/raw_to_standardized.py'
    )

    # Quality checks
    quality_check = PythonOperator(
        task_id='quality_check',
        python_callable=run_soda_scan,
        op_kwargs={'dataset': 'standardized.users'}
    )

    # Curate (business logic)
    curate = SparkSubmitOperator(
        task_id='curate_users',
        application='pipelines/transformation/standardized_to_curated.py'
    )

    # Promote to Trusted
    promote_to_trusted = SparkSubmitOperator(
        task_id='promote_to_trusted',
        application='pipelines/transformation/curated_to_trusted.py'
    )

    ingest_users >> standardize >> quality_check >> curate >> promote_to_trusted
```

## Directory Structure

```
pipeline/
├── pipelines/
│   ├── ingestion/
│   │   ├── postgres_to_raw.py
│   │   ├── s3_to_raw.py
│   │   └── api_to_raw.py
│   ├── transformation/
│   │   ├── raw_to_standardized.py
│   │   ├── standardized_to_curated.py
│   │   └── curated_to_trusted.py
│   └── streaming/
│       ├── kafka_to_delta.py
│       └── real_time_aggregation.py
├── dags/
│   ├── daily_data_pipeline.py
│   ├── hourly_streaming_pipeline.py
│   └── backfill_pipeline.py
├── common/
│   ├── quality_gates.py
│   ├── transformations.py
│   └── utils.py
├── tests/
├── config/
└── README.md
```

## Quality Gates

Each zone transition includes quality gates:

```python
# common/quality_gates.py
from soda.scan import Scan

def quality_gate(zone, dataset_name):
    scan = Scan()
    scan.add_configuration_yaml_file(f"config/{zone}_quality.yml")
    scan.add_sodacl_yaml_file(f"checks/{dataset_name}.yml")

    exit_code = scan.execute()

    if exit_code != 0:
        raise ValueError(f"Quality check failed for {dataset_name} in {zone}")

    return True
```

## Deployment

```bash
# Build Docker image
docker build -t data-pipeline:latest .

# Deploy to Kubernetes
kubectl apply -f k8s/spark-operator.yaml
kubectl apply -f k8s/airflow.yaml
```

## Monitoring

- Pipeline success/failure rates
- Processing latency
- Data volume metrics
- Quality check results

## References

- [Apache Spark Documentation](https://spark.apache.org/docs/latest/)
- [Apache Airflow Documentation](https://airflow.apache.org/docs/)
- [Delta Lake Documentation](https://docs.delta.io/)
