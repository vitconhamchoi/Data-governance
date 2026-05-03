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
    amount DECIMAL(10,2)
);

CREATE TABLE IF NOT EXISTS policies (
    id BIGSERIAL PRIMARY KEY,
    dataset VARCHAR(100) NOT NULL,
    column_name VARCHAR(100) NOT NULL,
    rule VARCHAR(20) NOT NULL,
    role VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample users
INSERT INTO users (name, email, phone) VALUES
('Alice', 'alice@example.com', '0901234567'),
('Bob', 'bob@example.com', '0912345678'),
('Charlie', 'charlie@example.com', '0923456789'),
('Diana', 'diana@example.com', '0934567890'),
('Eve', 'eve@example.com', '0945678901')
ON CONFLICT DO NOTHING;

-- Insert sample orders
INSERT INTO orders (user_id, amount) VALUES
(1, 150.00),
(1, 200.50),
(2, 75.25),
(3, 300.00),
(4, 99.99),
(5, 450.00)
ON CONFLICT DO NOTHING;

-- Insert default policies
INSERT INTO policies (dataset, column_name, rule, role) VALUES
('users', 'email', 'MASK', 'analyst'),
('users', 'phone', 'MASK', 'analyst')
ON CONFLICT DO NOTHING;
