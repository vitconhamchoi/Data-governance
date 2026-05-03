#!/bin/bash

# Data Governance System - Setup Script

set -e

echo "================================================"
echo "Data Governance System Setup"
echo "================================================"

# Check prerequisites
echo ""
echo "Checking prerequisites..."
command -v docker >/dev/null 2>&1 || { echo "Docker is required but not installed. Aborting." >&2; exit 1; }
command -v docker-compose >/dev/null 2>&1 || command -v docker compose >/dev/null 2>&1 || { echo "Docker Compose is required but not installed. Aborting." >&2; exit 1; }

echo "✓ Docker found"
echo "✓ Docker Compose found"

# Create necessary directories
echo ""
echo "Creating directories..."
mkdir -p trino/catalog
mkdir -p airflow/dags
mkdir -p airflow/plugins
mkdir -p dbt/models
mkdir -p data
mkdir -p scripts

echo "✓ Directories created"

# Start services
echo ""
echo "Starting Docker services..."
echo "This may take several minutes on first run..."
docker-compose up -d

echo ""
echo "Waiting for services to be ready..."
sleep 30

# Check service health
echo ""
echo "Checking service health..."

# Check PostgreSQL
echo -n "PostgreSQL: "
if docker exec postgres pg_isready -U datauser > /dev/null 2>&1; then
    echo "✓ Ready"
else
    echo "✗ Not ready"
fi

# Check Trino
echo -n "Trino: "
if curl -s http://localhost:8080/v1/info > /dev/null 2>&1; then
    echo "✓ Ready"
else
    echo "✗ Not ready (may need more time)"
fi

# Check Airflow
echo -n "Airflow: "
if curl -s http://localhost:8081/health > /dev/null 2>&1; then
    echo "✓ Ready"
else
    echo "✗ Not ready (may need more time)"
fi

# Check Policy Service
echo -n "Policy Service: "
if curl -s http://localhost:5001/swagger > /dev/null 2>&1; then
    echo "✓ Ready"
else
    echo "✗ Not ready (may need more time)"
fi

# Check Query Gateway
echo -n "Query Gateway: "
if curl -s http://localhost:5002/swagger > /dev/null 2>&1; then
    echo "✓ Ready"
else
    echo "✗ Not ready (may need more time)"
fi

echo ""
echo "================================================"
echo "Setup Complete!"
echo "================================================"
echo ""
echo "Access URLs:"
echo "  - Airflow:         http://localhost:8081 (admin/admin)"
echo "  - Trino:           http://localhost:8080"
echo "  - DataHub:         http://localhost:9002"
echo "  - MinIO Console:   http://localhost:9001 (minioadmin/minioadmin123)"
echo "  - Policy API:      http://localhost:5001/swagger"
echo "  - Query Gateway:   http://localhost:5002/swagger"
echo ""
echo "Next steps:"
echo "  1. Run: ./scripts/demo.sh"
echo "  2. Check the RUN_GUIDE.md for detailed instructions"
echo ""
