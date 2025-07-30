#!/bin/bash

echo "Stopping Drug Prevention System..."

echo ""
echo "=== Stopping all containers ==="
docker-compose down

echo ""
echo "=== Container status ==="
docker-compose ps

echo ""
echo "All services stopped!"