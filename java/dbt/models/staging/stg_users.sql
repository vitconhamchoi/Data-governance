{{ config(materialized='view') }}

SELECT
    id,
    name,
    email,
    phone,
    CURRENT_TIMESTAMP AS _loaded_at
FROM {{ source('public', 'users') }}
