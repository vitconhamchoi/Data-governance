-- Load sample data from CSV (manual execution example)

-- This script shows how to load CSV data into PostgreSQL
-- In production, this would be done via Airflow

-- Users data is already loaded via init-db.sql
-- Orders data is already loaded via init-db.sql

-- Additional sample data for testing

INSERT INTO users (name, email, phone) VALUES
('David Lee', 'david.lee@example.com', '+1-555-0106'),
('Emma Davis', 'emma.davis@example.com', '+1-555-0107'),
('Frank Miller', 'frank.miller@example.com', '+1-555-0108'),
('Grace Taylor', 'grace.taylor@example.com', '+1-555-0109'),
('Henry Wilson', 'henry.wilson@example.com', '+1-555-0110')
ON CONFLICT DO NOTHING;

INSERT INTO orders (user_id, amount) VALUES
(6, 129.99),
(7, 89.50),
(8, 199.00),
(9, 59.99),
(10, 299.99)
ON CONFLICT DO NOTHING;

-- Verify data
SELECT COUNT(*) as user_count FROM users;
SELECT COUNT(*) as order_count FROM orders;
