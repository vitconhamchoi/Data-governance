# Database Integrations

## Overview

Database connectors and adapters for PostgreSQL, TimescaleDB, ClickHouse and other databases.

## Supported Databases

- **PostgreSQL**: Primary OLTP database
- **TimescaleDB**: Time-series data storage
- **ClickHouse**: Analytics and OLAP queries
- **Redis**: Caching and session storage

## Features

- Connection pooling
- Automatic retries
- Query monitoring
- Migration management
- Metadata extraction

## Configuration

```yaml
databases:
  postgres:
    host: localhost
    port: 5432
    database: governance
    user: ${DB_USER}
    password: ${DB_PASSWORD}
    pool_size: 20

  timescaledb:
    host: localhost
    port: 5432
    database: telemetry
    hypertables:
      - measurements
      - events

  clickhouse:
    host: localhost
    port: 9000
    database: analytics
```

## Usage

```python
from integrations.databases import PostgresConnector

db = PostgresConnector(config)
results = db.query("SELECT * FROM users LIMIT 10")
```
