-- models/users_with_orders.sql
-- Transform users data with order aggregations

SELECT
    u.id,
    u.name,
    u.email,
    u.phone,
    COUNT(o.id) as total_orders,
    COALESCE(SUM(o.amount), 0) as total_amount,
    u.created_at
FROM {{ source('public', 'users') }} u
LEFT JOIN {{ source('public', 'orders') }} o ON u.id = o.user_id
GROUP BY u.id, u.name, u.email, u.phone, u.created_at
