-- Create sample tables
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS orders (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    amount DECIMAL(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample data
INSERT INTO users (name, email, phone) VALUES
('John Doe', 'john.doe@example.com', '+1-555-0101'),
('Jane Smith', 'jane.smith@example.com', '+1-555-0102'),
('Bob Johnson', 'bob.johnson@example.com', '+1-555-0103'),
('Alice Brown', 'alice.brown@example.com', '+1-555-0104'),
('Charlie Wilson', 'charlie.wilson@example.com', '+1-555-0105')
ON CONFLICT DO NOTHING;

INSERT INTO orders (user_id, amount) VALUES
(1, 99.99),
(1, 149.50),
(2, 299.00),
(3, 49.99),
(4, 199.99),
(5, 79.99)
ON CONFLICT DO NOTHING;

-- Create metadata tracking table
CREATE TABLE IF NOT EXISTS data_quality_checks (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    check_name VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL,
    result_details TEXT,
    checked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
