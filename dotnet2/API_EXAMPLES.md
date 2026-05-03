# API Examples - Data Governance System

## Policy Service API (Port 5001)

### 1. Create Masking Policy for Email

```bash
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "email",
    "rule": "mask",
    "role": "analyst"
  }'
```

**Response:**
```json
{
  "id": 1,
  "dataset": "users",
  "column": "email",
  "rule": "mask",
  "role": "analyst",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### 2. Create Masking Policy for Phone

```bash
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "phone",
    "rule": "mask",
    "role": "analyst"
  }'
```

### 3. Create Deny Policy

```bash
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "phone",
    "rule": "deny",
    "role": "viewer"
  }'
```

### 4. Get All Policies

```bash
curl -X GET http://localhost:5001/api/policies | jq '.'
```

**Response:**
```json
[
  {
    "id": 1,
    "dataset": "users",
    "column": "email",
    "rule": "mask",
    "role": "analyst",
    "createdAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": 2,
    "dataset": "users",
    "column": "phone",
    "rule": "mask",
    "role": "analyst",
    "createdAt": "2024-01-15T10:31:00Z"
  }
]
```

### 5. Get Policies by Dataset and Role

```bash
curl -X GET http://localhost:5001/api/policies/dataset/users/role/analyst | jq '.'
```

### 6. Get Single Policy

```bash
curl -X GET http://localhost:5001/api/policies/1 | jq '.'
```

### 7. Update Policy

```bash
curl -X PUT http://localhost:5001/api/policies/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "dataset": "users",
    "column": "email",
    "rule": "deny",
    "role": "analyst"
  }'
```

### 8. Delete Policy

```bash
curl -X DELETE http://localhost:5001/api/policies/1
```

---

## Query Gateway API (Port 5002)

### 1. Query as Admin (No Masking)

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users LIMIT 3",
    "role": "admin",
    "dataset": "users"
  }' | jq '.'
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john.doe@example.com",
      "phone": "+1-555-0101"
    },
    {
      "id": 2,
      "name": "Jane Smith",
      "email": "jane.smith@example.com",
      "phone": "+1-555-0102"
    }
  ],
  "columns": ["id", "name", "email", "phone"],
  "rowCount": 2,
  "success": true,
  "appliedPolicies": []
}
```

### 2. Query as Analyst (With Masking)

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users LIMIT 3",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.'
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "j***@example.com",
      "phone": "****0101"
    },
    {
      "id": 2,
      "name": "Jane Smith",
      "email": "j***@example.com",
      "phone": "****0102"
    }
  ],
  "columns": ["id", "name", "email", "phone"],
  "rowCount": 2,
  "success": true,
  "appliedPolicies": ["email:mask", "phone:mask"]
}
```

### 3. Query with Aggregations

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT COUNT(*) as total, AVG(amount) as avg_amount FROM postgresql.public.orders",
    "role": "analyst",
    "dataset": "orders"
  }' | jq '.'
```

### 4. Query with JOIN

```bash
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT u.name, u.email, COUNT(o.id) as order_count FROM postgresql.public.users u LEFT JOIN postgresql.public.orders o ON u.id = o.user_id GROUP BY u.id, u.name, u.email",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.'
```

**Response:**
```json
{
  "data": [
    {
      "name": "John Doe",
      "email": "j***@example.com",
      "order_count": 2
    }
  ],
  "columns": ["name", "email", "order_count"],
  "rowCount": 5,
  "success": true,
  "appliedPolicies": ["email:mask"]
}
```

### 5. Query as Viewer (Denied Phone)

```bash
# First create deny policy
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "phone",
    "rule": "deny",
    "role": "viewer"
  }'

# Then query
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, phone FROM postgresql.public.users LIMIT 2",
    "role": "viewer",
    "dataset": "users"
  }' | jq '.'
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "phone": "[REDACTED]"
    }
  ],
  "columns": ["id", "name", "phone"],
  "rowCount": 2,
  "success": true,
  "appliedPolicies": ["phone:deny"]
}
```

---

## PostgreSQL Direct Access

### Connect to Database

```bash
docker exec -it postgres psql -U datauser -d datagovernance
```

### Sample Queries

```sql
-- View all users
SELECT * FROM users;

-- View all orders
SELECT * FROM orders;

-- View data quality check results
SELECT * FROM data_quality_checks ORDER BY checked_at DESC;

-- Check PII columns
SELECT id, name, email, phone FROM users WHERE id = 1;
```

---

## Complete Workflow Example

```bash
#!/bin/bash

# 1. Create policies for analyst role
echo "Creating masking policies..."
curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{"dataset": "users", "column": "email", "rule": "mask", "role": "analyst"}'

curl -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{"dataset": "users", "column": "phone", "rule": "mask", "role": "analyst"}'

# 2. Query as different roles
echo -e "\n\nAdmin query (full access):"
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users WHERE id = 1",
    "role": "admin",
    "dataset": "users"
  }' | jq '.data[0]'

echo -e "\n\nAnalyst query (masked):"
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users WHERE id = 1",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.data[0]'

# 3. Show applied policies
echo -e "\n\nApplied policies:"
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users WHERE id = 1",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.appliedPolicies'

# 4. List all policies
echo -e "\n\nAll policies:"
curl -X GET http://localhost:5001/api/policies | jq '.'
```

---

## Postman Collection

Import these into Postman:

**Environment Variables:**
```json
{
  "policy_service_url": "http://localhost:5001",
  "query_gateway_url": "http://localhost:5002"
}
```

**Collection:**
```json
{
  "info": {
    "name": "Data Governance API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Policy Service",
      "item": [
        {
          "name": "Create Policy",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "body": {
              "mode": "raw",
              "raw": "{\"dataset\":\"users\",\"column\":\"email\",\"rule\":\"mask\",\"role\":\"analyst\"}"
            },
            "url": "{{policy_service_url}}/api/policies"
          }
        },
        {
          "name": "Get All Policies",
          "request": {
            "method": "GET",
            "url": "{{policy_service_url}}/api/policies"
          }
        }
      ]
    },
    {
      "name": "Query Gateway",
      "item": [
        {
          "name": "Execute Query",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "body": {
              "mode": "raw",
              "raw": "{\"sql\":\"SELECT * FROM postgresql.public.users LIMIT 3\",\"role\":\"analyst\",\"dataset\":\"users\"}"
            },
            "url": "{{query_gateway_url}}/api/query"
          }
        }
      ]
    }
  ]
}
```

---

## Testing Different Scenarios

### Scenario 1: Progressive Access Levels

```bash
# Admin - sees everything
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT * FROM postgresql.public.users LIMIT 1", "role": "admin", "dataset": "users"}' | jq '.data[0]'

# Analyst - sees masked PII
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT * FROM postgresql.public.users LIMIT 1", "role": "analyst", "dataset": "users"}' | jq '.data[0]'

# Viewer - sees denied fields as [REDACTED]
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT * FROM postgresql.public.users LIMIT 1", "role": "viewer", "dataset": "users"}' | jq '.data[0]'
```

### Scenario 2: Dynamic Policy Updates

```bash
# Create initial mask policy
POLICY_ID=$(curl -s -X POST http://localhost:5001/api/policies \
  -H "Content-Type: application/json" \
  -d '{"dataset": "users", "column": "email", "rule": "mask", "role": "analyst"}' | jq -r '.id')

# Query with masking
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT email FROM postgresql.public.users LIMIT 1", "role": "analyst", "dataset": "users"}' | jq '.data[0].email'

# Update to deny policy
curl -X PUT http://localhost:5001/api/policies/$POLICY_ID \
  -H "Content-Type: application/json" \
  -d "{\"id\": $POLICY_ID, \"dataset\": \"users\", \"column\": \"email\", \"rule\": \"deny\", \"role\": \"analyst\"}"

# Query again - now denied
curl -X POST http://localhost:5002/api/query \
  -H "Content-Type: application/json" \
  -d '{"sql": "SELECT email FROM postgresql.public.users LIMIT 1", "role": "analyst", "dataset": "users"}' | jq '.data[0].email'
```
