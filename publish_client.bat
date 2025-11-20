@echo off
echo Publishing EncryptItVC Client...
cd Client
dotnet publish -c Release -r win-x64 --self-contained true
echo Client published to bin\Release\net6.0-windows\win-x64\publish\
pause
