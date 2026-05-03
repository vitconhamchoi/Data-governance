# Data Scanner - Automated Discovery & Classification

## Overview

Automated data discovery, profiling, and classification service that scans databases and files to extract metadata and identify sensitive data.

## Features

- **Auto-Discovery**: Scan databases and data lakes for new tables/datasets
- **Data Profiling**: Statistical analysis (min, max, avg, null%, cardinality)
- **PII Detection**: Identify sensitive data (emails, SSN, credit cards, phone numbers)
- **Schema Extraction**: Capture table schemas and column metadata
- **Drift Detection**: Detect schema changes over time

## Architecture

```
Scheduler → Scanner → Classifier → Metadata Store
    ↓          ↓          ↓            ↓
 Cron/API   Extract   ML Models    DataHub
             Profile   Patterns     Postgres
             Analyze   Rules
```

## Configuration

```yaml
scanner:
  schedule: "0 2 * * *"  # Daily at 2 AM
  targets:
    - type: postgres
      connection: ${POSTGRES_CONN}
      schemas: [public, analytics]

    - type: s3
      bucket: data-lake
      paths: [raw/*, curated/*]

  classifiers:
    - name: pii_detector
      patterns:
        - email: "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"
        - ssn: "^\\d{3}-\\d{2}-\\d{4}$"
        - phone: "^\\+?1?\\d{9,15}$"
        - credit_card: "^\\d{4}[\\s-]?\\d{4}[\\s-]?\\d{4}[\\s-]?\\d{4}$"

    - name: ml_classifier
      model: distilbert-base-uncased-finetuned-pii
```

## Usage

```python
from automation.data_scanner import DataScanner

# Initialize scanner
scanner = DataScanner(config)

# Scan a database
results = scanner.scan_database(
    connection_string="postgresql://user:pass@host/db",
    schemas=["public", "analytics"]
)

# Results include:
# - Tables discovered
# - Columns profiled
# - PII detected
# - Metadata extracted

for table in results.tables:
    print(f"Table: {table.name}")
    print(f"Row count: {table.row_count}")

    for column in table.columns:
        print(f"  {column.name}: {column.type}")
        if column.is_pii:
            print(f"    ⚠️ PII detected: {column.pii_type}")
        print(f"    Null%: {column.null_percentage}")
        print(f"    Unique values: {column.cardinality}")
```

## PII Detection

```python
# Automatic PII detection
scanner.detect_pii(
    dataset="postgres.public.users",
    columns=["email", "phone", "ssn", "address"]
)

# Custom regex patterns
scanner.add_pii_pattern(
    name="custom_id",
    regex="^CUS-\\d{8}$",
    description="Customer ID format"
)
```

## Metadata Publishing

```python
# Publish discovered metadata to DataHub
scanner.publish_to_datahub(scan_results)

# Publish to internal metadata store
scanner.publish_to_postgres(scan_results)
```

## Scheduling

```python
# Airflow DAG
from airflow import DAG
from airflow.operators.python import PythonOperator

with DAG('data_scanner_daily', schedule_interval='@daily') as dag:
    scan_task = PythonOperator(
        task_id='scan_databases',
        python_callable=run_scanner
    )
```

## Directory Structure

```
data-scanner/
├── scanner/
│   ├── database_scanner.py
│   ├── file_scanner.py
│   └── schema_extractor.py
├── classifiers/
│   ├── pii_detector.py
│   ├── ml_classifier.py
│   └── pattern_matcher.py
├── profilers/
│   ├── statistical_profiler.py
│   └── quality_profiler.py
├── publishers/
│   ├── datahub_publisher.py
│   └── metadata_publisher.py
├── config/
├── tests/
└── README.md
```

## Monitoring

- Datasets scanned per day
- PII columns discovered
- Schema drift detected
- Scan duration and errors
