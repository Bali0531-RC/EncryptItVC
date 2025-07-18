@echo off
echo Building EncryptItVC Client...
cd Client
dotnet restore
dotnet build -c Release
echo Client build completed!
pause
