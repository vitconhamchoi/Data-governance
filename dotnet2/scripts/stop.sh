#!/bin/bash

# Stop and remove all containers
echo "Stopping all services..."
docker-compose down

# Optional: Remove volumes (use with caution - this deletes all data)
if [ "$1" == "--volumes" ]; then
    echo "Removing all volumes (this will delete all data)..."
    docker-compose down -v
fi

echo "All services stopped."
