#!/bin/bash

echo "Building and starting EncryptItVC Server with Docker..."

# Build the server first
echo "Building server..."
cd Server
dotnet build -c Release
cd ..

# Build and run with Docker Compose
echo "Starting Docker container..."
docker-compose up -d

echo "Server started! Check status with: docker-compose ps"
echo "View logs with: docker-compose logs -f"
echo "Stop with: docker-compose down"
