CREATE EXTENSION IF NOT EXISTS vector;

-- Create schema for governance data
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150),
    phone VARCHAR(20)
);

CREATE TABLE IF NOT EXISTS orders (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    amount DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS policies (
    id BIGSERIAL PRIMARY KEY,
    dataset VARCHAR(100) NOT NULL,
    column_name VARCHAR(100) NOT NULL,
    rule VARCHAR(20) NOT NULL,
    role VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS policy_suggestions (
    id BIGSERIAL PRIMARY KEY,
    dataset VARCHAR(100) NOT NULL,
    column_name VARCHAR(100) NOT NULL,
    tag VARCHAR(100) NOT NULL,
    suggested_rule VARCHAR(20) NOT NULL,
    reason TEXT NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    approved_by VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    approved_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS dataset_embeddings (
    dataset VARCHAR(255) PRIMARY KEY,
    metadata_json JSONB NOT NULL DEFAULT '{}'::jsonb,
    embedding vector(1536) NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample users
INSERT INTO users (name, email, phone) VALUES
('Alice Nguyen', 'alice@example.com', '0901234567'),
('Bob Tran', 'bob@example.com', '0912345678'),
('Charlie Pham', '0923456789', '0923456789'),
('Diana Le', 'diana@example.com', '0934567890'),
('Eve Hoang', 'eve@example.com', '0945678901')
ON CONFLICT DO NOTHING;

-- Insert sample orders
INSERT INTO orders (user_id, amount, created_at) VALUES
(1, 150.00, CURRENT_TIMESTAMP - INTERVAL '60 days'),
(1, 200.50, CURRENT_TIMESTAMP - INTERVAL '35 days'),
(2, 75.25, CURRENT_TIMESTAMP - INTERVAL '15 days'),
(3, 300.00, CURRENT_TIMESTAMP - INTERVAL '12 days'),
(4, 99.99, CURRENT_TIMESTAMP - INTERVAL '3 days'),
(5, 450.00, CURRENT_TIMESTAMP - INTERVAL '1 day')
ON CONFLICT DO NOTHING;

-- Insert default policies
INSERT INTO policies (dataset, column_name, rule, role) VALUES
('users', 'email', 'MASK', 'analyst'),
('users', 'phone', 'MASK', 'analyst')
ON CONFLICT DO NOTHING;
