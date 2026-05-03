{{ config(materialized='table') }}

SELECT
    u.id AS user_id,
    u.name AS user_name,
    u.email,
    COUNT(o.id) AS total_orders,
    SUM(o.amount) AS total_amount
FROM {{ ref('stg_users') }} u
LEFT JOIN {{ ref('stg_orders') }} o ON u.id = o.user_id
GROUP BY u.id, u.name, u.email
