#!/bin/bash

echo "Building EncryptItVC Server..."
cd Server
dotnet restore
dotnet build -c Release
echo "Server build completed!"
read -p "Press Enter to continue..."
