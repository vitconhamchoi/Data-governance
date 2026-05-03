# ETL Integration

## Overview

Integration with ETL/ELT tools including Airbyte, dbt, and Fivetran for data movement and transformation.

## Supported Tools

- **Airbyte**: Open-source data integration
- **dbt**: SQL-based transformation
- **Fivetran**: Managed data pipelines

## dbt Integration

```yaml
# profiles.yml
governance:
  target: prod
  outputs:
    prod:
      type: postgres
      host: warehouse.example.com
      database: analytics
      schema: public
```

```sql
-- models/marts/user_metrics.sql
{{ config(materialized='table') }}

SELECT
    u.user_id,
    u.email,
    COUNT(DISTINCT o.order_id) as total_orders,
    SUM(o.amount) as lifetime_value
FROM {{ source('raw', 'users') }} u
LEFT JOIN {{ source('raw', 'orders') }} o
    ON u.user_id = o.user_id
GROUP BY u.user_id, u.email
```

## Airbyte Integration

```json
{
  "source": {
    "type": "postgres",
    "config": {
      "host": "source-db.example.com",
      "database": "production"
    }
  },
  "destination": {
    "type": "s3",
    "config": {
      "bucket": "data-lake",
      "prefix": "raw/"
    }
  }
}
```
