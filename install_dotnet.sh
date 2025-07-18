#!/bin/bash

echo "Checking and installing .NET Runtime..."

# Check if .NET is already installed
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version 2>/dev/null | cut -d'.' -f1)
    echo "Found .NET version: $(dotnet --version)"
    
    if [ "$DOTNET_VERSION" -ge 6 ]; then
        echo "✅ .NET $DOTNET_VERSION is already installed and compatible!"
        echo "You can run the server with: ./start_server.sh"
        exit 0
    else
        echo "⚠️  .NET version is too old, installing newer version..."
    fi
else
    echo "🔍 .NET not found, installing..."
fi

# Detect OS and install
if [ -f /etc/debian_version ]; then
    echo "Detected Debian/Ubuntu system"
    sudo apt update
    sudo apt install -y dotnet-runtime-6.0
elif [ -f /etc/redhat-release ]; then
    echo "Detected Red Hat/CentOS/Fedora system"
    sudo dnf install -y dotnet-runtime-6.0
elif [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Detected macOS system"
    if command -v brew &> /dev/null; then
        brew install dotnet
    else
        echo "Homebrew not found. Please install Homebrew first or install .NET manually."
        exit 1
    fi
else
    echo "Unknown OS. Please install .NET 6.0+ Runtime manually."
    echo "Visit: https://dotnet.microsoft.com/download/dotnet"
    exit 1
fi

echo "✅ .NET Runtime installation completed!"
echo "You can now run the server with: ./start_server.sh"
