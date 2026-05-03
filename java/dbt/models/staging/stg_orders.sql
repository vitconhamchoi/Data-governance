{{ config(materialized='view') }}

SELECT
    id,
    user_id,
    amount,
    CURRENT_TIMESTAMP AS _loaded_at
FROM {{ source('public', 'orders') }}
