#!/bin/bash

# Check service status
echo "================================================"
echo "Data Governance System - Service Status"
echo "================================================"
echo ""

# Function to check HTTP endpoint
check_http() {
    local name=$1
    local url=$2
    echo -n "$name: "
    if curl -s -f -o /dev/null "$url"; then
        echo "✓ Running"
    else
        echo "✗ Not accessible"
    fi
}

# Function to check container
check_container() {
    local name=$1
    echo -n "$name: "
    if docker ps --format '{{.Names}}' | grep -q "^${name}$"; then
        echo "✓ Running"
    else
        echo "✗ Not running"
    fi
}

echo "Container Status:"
check_container "postgres"
check_container "trino"
check_container "datahub-gms"
check_container "datahub-frontend"
check_container "airflow-webserver"
check_container "airflow-scheduler"
check_container "policy-service"
check_container "query-gateway"
check_container "minio"
check_container "kafka"

echo ""
echo "Service Health:"
check_http "PostgreSQL" "http://localhost:5432"
check_http "Trino" "http://localhost:8080/v1/info"
check_http "Airflow" "http://localhost:8081/health"
check_http "DataHub" "http://localhost:9002"
check_http "Policy Service" "http://localhost:5001/swagger/index.html"
check_http "Query Gateway" "http://localhost:5002/swagger/index.html"
check_http "MinIO" "http://localhost:9001"

echo ""
echo "View logs with: docker-compose logs -f [service-name]"
echo ""
