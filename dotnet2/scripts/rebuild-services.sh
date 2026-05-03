#!/bin/bash

# Rebuild and restart .NET services

echo "Rebuilding .NET services..."

docker-compose stop policy-service query-gateway
docker-compose build --no-cache policy-service query-gateway
docker-compose up -d policy-service query-gateway

echo ""
echo "Waiting for services to start..."
sleep 10

echo ""
echo "Services restarted. Check status:"
docker-compose ps policy-service query-gateway

echo ""
echo "View logs:"
echo "  docker-compose logs -f policy-service"
echo "  docker-compose logs -f query-gateway"
