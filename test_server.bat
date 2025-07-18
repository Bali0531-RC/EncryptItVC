@echo off
echo === EncryptItVC Server Test ===
echo.
echo Testing server connection...
echo.

REM Test TCP connection
echo Testing TCP connection on port 7777...
telnet localhost 7777

echo.
echo Press any key to continue...
pause >nul
