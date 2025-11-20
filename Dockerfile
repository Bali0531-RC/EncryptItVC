FROM mcr.microsoft.com/dotnet/runtime:9.0

WORKDIR /app

# Copy server files
COPY Server/bin/Release/net9.0/ .

# Expose ports
EXPOSE 7777 7778

# Run the server
ENTRYPOINT ["dotnet", "EncryptItVC.Server.dll"]
