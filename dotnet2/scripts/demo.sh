#!/bin/bash

# Data Governance System - Demo Script

set -e

echo "================================================"
echo "Data Governance Demo Scenario"
echo "================================================"

API_BASE_POLICY="http://localhost:5001/api/policies"
API_BASE_QUERY="http://localhost:5002/api/query"

# Step 1: Create policies
echo ""
echo "Step 1: Creating data governance policies..."

# Policy 1: Analysts cannot see full email
echo "Creating policy: analyst role - mask email"
curl -X POST "$API_BASE_POLICY" \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "email",
    "rule": "mask",
    "role": "analyst"
  }'

echo ""

# Policy 2: Analysts cannot see full phone
echo "Creating policy: analyst role - mask phone"
curl -X POST "$API_BASE_POLICY" \
  -H "Content-Type: application/json" \
  -d '{
    "dataset": "users",
    "column": "phone",
    "rule": "mask",
    "role": "analyst"
  }'

echo ""
echo "✓ Policies created"

# Step 2: Query as admin (no masking)
echo ""
echo "Step 2: Query as admin (full data access)..."
echo ""
curl -X POST "$API_BASE_QUERY" \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users LIMIT 3",
    "role": "admin",
    "dataset": "users"
  }' | jq '.'

# Step 3: Query as analyst (with masking)
echo ""
echo "Step 3: Query as analyst (masked PII)..."
echo ""
curl -X POST "$API_BASE_QUERY" \
  -H "Content-Type: application/json" \
  -d '{
    "sql": "SELECT id, name, email, phone FROM postgresql.public.users LIMIT 3",
    "role": "analyst",
    "dataset": "users"
  }' | jq '.'

# Step 4: Show all policies
echo ""
echo "Step 4: View all active policies..."
echo ""
curl -X GET "$API_BASE_POLICY" | jq '.'

echo ""
echo "================================================"
echo "Demo Complete!"
echo "================================================"
echo ""
echo "What happened:"
echo "  1. Created policies to mask email and phone for analysts"
echo "  2. Admin query showed full data"
echo "  3. Analyst query showed masked PII data"
echo "  4. Listed all active policies"
echo ""
echo "Key observations:"
echo "  - Email: john.doe@example.com → j***@example.com"
echo "  - Phone: +1-555-0101 → ****0101"
echo ""
echo "Try modifying policies and running queries with different roles!"
echo ""
