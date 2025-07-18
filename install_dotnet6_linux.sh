#!/bin/bash
# EncryptItVC Server .NET 6.0 telepítő script Linux-ra

echo "=== EncryptItVC Server .NET 6.0 Setup ==="
echo

# .NET 6.0 runtime telepítése
echo "Installing .NET 6.0 runtime..."
sudo apt-get update
sudo apt-get install -y dotnet-runtime-6.0

# Ellenőrzés
echo
echo "Checking .NET versions..."
dotnet --list-runtimes

echo
echo "Testing server..."
cd ~/EncryptItVC/Server/bin/Release/net6.0
./EncryptItVC.Server

echo
echo "Setup completed!"
