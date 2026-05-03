# Data Quality Engine - Soda Integration

## Overview

This module implements data quality monitoring and validation using Soda Core, an open-source framework for data reliability.

## Architecture

```
┌─────────────────────────────────────────────────┐
│          Soda Core Quality Engine               │
├─────────────────────────────────────────────────┤
│  • Quality Checks Definition (YAML)             │
│  • Data Profiling                               │
│  • Anomaly Detection                            │
│  • Custom Validators                            │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│         Quality Check Execution                 │
├─────────────────────────────────────────────────┤
│  • Scheduled Scans (Airflow)                    │
│  • On-Demand Scans (API)                        │
│  • Pipeline Integration (dbt tests)             │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│         Results & Alerts                        │
├─────────────────────────────────────────────────┤
│  • Quality Metrics Storage                      │
│  • Alert Notifications (Slack, PagerDuty)       │
│  • Dashboard Visualization                      │
│  • Metadata Store Integration                  │
└─────────────────────────────────────────────────┘
```

## Features

- **Automated Data Profiling**: Statistical analysis of datasets
- **Quality Checks**: Completeness, validity, consistency, accuracy
- **Anomaly Detection**: ML-based detection of data anomalies
- **Schema Validation**: Monitor schema changes
- **Custom Checks**: Define business-specific validations

## Installation

```bash
pip install soda-core-postgres soda-core-spark
```

## Configuration

### configuration.yml

```yaml
data_source my_postgres:
  type: postgres
  host: localhost
  port: 5432
  database: mydb
  username: ${POSTGRES_USER}
  password: ${POSTGRES_PASSWORD}

soda_cloud:
  api_key_id: ${SODA_API_KEY_ID}
  api_key_secret: ${SODA_API_KEY_SECRET}
```

## Quality Checks Definition

### checks/users_table.yml

```yaml
checks for users:
  # Completeness checks
  - missing_count(email) = 0:
      name: Email should not be null
  - missing_percent(phone) < 5%:
      name: Less than 5% missing phone numbers

  # Validity checks
  - invalid_count(email) = 0:
      valid format: email
      name: All emails must be valid format

  - invalid_percent(age) < 1%:
      valid min: 18
      valid max: 120
      name: Age must be between 18-120

  # Uniqueness checks
  - duplicate_count(user_id) = 0:
      name: User IDs must be unique

  # Freshness checks
  - freshness(created_at) < 1d:
      name: Data should be less than 1 day old

  # Custom SQL checks
  - failed rows:
      name: Active users must have verified email
      fail query: |
        SELECT *
        FROM users
        WHERE status = 'active'
          AND email_verified = false

  # Schema checks
  - schema:
      fail:
        when required column missing: [user_id, email, created_at]
        when wrong column type:
          email: varchar
          created_at: timestamp
```

### checks/transactions_table.yml

```yaml
checks for transactions:
  - row_count > 0:
      name: Table should not be empty

  - avg(amount) between 10 and 10000:
      name: Average transaction amount in expected range

  - missing_count(transaction_id) = 0
  - duplicate_count(transaction_id) = 0

  - invalid_count(status) = 0:
      valid values: ['pending', 'completed', 'failed', 'cancelled']

  # Anomaly detection
  - anomaly detection for row_count:
      name: Detect unusual transaction volumes

  # Cross-table validation
  - failed rows:
      name: All transactions must have valid user
      fail query: |
        SELECT t.*
        FROM transactions t
        LEFT JOIN users u ON t.user_id = u.user_id
        WHERE u.user_id IS NULL
```

## Usage

### Run Quality Scans

```bash
# Scan a single table
soda scan -d my_postgres -c configuration.yml checks/users_table.yml

# Scan all checks
soda scan -d my_postgres -c configuration.yml checks/

# Scan with variables
soda scan -d my_postgres -c configuration.yml -v date=2024-01-01 checks/
```

### Python API

```python
from soda.scan import Scan

scan = Scan()
scan.set_data_source_name("my_postgres")
scan.add_configuration_yaml_file("configuration.yml")
scan.add_sodacl_yaml_file("checks/users_table.yml")

# Execute scan
exit_code = scan.execute()

# Get results
scan_results = scan.get_scan_results()
print(f"Checks passed: {scan_results['checks_passed']}")
print(f"Checks failed: {scan_results['checks_failed']}")

# Get failed check details
for check in scan.get_checks_fail():
    print(f"Failed: {check.name}")
    print(f"Metric: {check.metrics}")
```

### Integration with Airflow

```python
from airflow import DAG
from airflow.operators.python import PythonOperator
from datetime import datetime, timedelta

def run_data_quality_checks():
    from soda.scan import Scan

    scan = Scan()
    scan.set_data_source_name("my_postgres")
    scan.add_configuration_yaml_file("configuration.yml")
    scan.add_sodacl_yaml_files("checks/")

    exit_code = scan.execute()

    if exit_code != 0:
        raise ValueError("Data quality checks failed!")

with DAG(
    'data_quality_daily',
    default_args={'retries': 1},
    schedule_interval='@daily',
    start_date=datetime(2024, 1, 1),
    catchup=False
) as dag:

    quality_check = PythonOperator(
        task_id='run_quality_checks',
        python_callable=run_data_quality_checks
    )
```

## Quality Metrics

The system tracks:
- **Completeness**: % of non-null values
- **Validity**: % of values matching constraints
- **Accuracy**: % of values matching expected patterns
- **Consistency**: Cross-table referential integrity
- **Timeliness**: Data freshness metrics
- **Uniqueness**: Duplicate detection

## Alerting

### Slack Integration

```yaml
# configuration.yml
soda_cloud:
  api_key_id: ${SODA_API_KEY_ID}
  api_key_secret: ${SODA_API_KEY_SECRET}

notifications:
  slack:
    webhook_url: ${SLACK_WEBHOOK_URL}
    channel: "#data-quality-alerts"
```

### Custom Alerts

```python
from soda.scan import Scan

def send_alert(check_result):
    if check_result.outcome == "fail":
        # Send to monitoring system
        send_to_pagerduty(check_result)
        send_to_slack(check_result)

scan = Scan()
# ... configure scan ...
scan.execute()

for check in scan.get_checks():
    send_alert(check)
```

## Directory Structure

```
data-quality/
├── config/
│   ├── configuration.yml
│   └── datasources/
│       ├── postgres.yml
│       ├── snowflake.yml
│       └── spark.yml
├── checks/
│   ├── raw_zone/
│   ├── standardized_zone/
│   ├── curated_zone/
│   └── trusted_zone/
├── profiles/
│   └── generated_profiles/
├── scripts/
│   ├── run_scans.sh
│   └── generate_report.py
└── README.md
```

## Integration with Data Platform

- **Lakehouse Zones**: Quality gates between zones (Raw → Standardized → Curated → Trusted)
- **Metadata Store**: Quality metrics visible in DataHub
- **Pipeline**: Quality checks in ETL workflows
- **Monitoring**: Quality metrics exposed to observability stack

## Monitoring

Quality metrics exposed via:
- Prometheus metrics endpoint
- Time-series database for trend analysis
- Grafana dashboards

## References

- [Soda Core Documentation](https://docs.soda.io/)
- [Soda Checks Reference](https://docs.soda.io/soda-cl/soda-cl-overview.html)
