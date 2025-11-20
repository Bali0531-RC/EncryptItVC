@echo off
echo === EncryptItVC Server Test ===
echo.
echo Testing server connection...
echo.

REM Test if server is running
echo Testing if server is running on port 7777...
netstat -an | findstr :7777 >nul
if %errorlevel% == 0 (
    echo ✅ Server appears to be running on port 7777
) else (
    echo ❌ Server is not running on port 7777
)

echo.
echo Testing UDP port 7778...
netstat -an | findstr :7778 >nul
if %errorlevel% == 0 (
    echo ✅ UDP port 7778 is in use (voice channel)
) else (
    echo ❌ UDP port 7778 is not in use
)

echo.
echo Testing connectivity to localhost...
ping -n 1 127.0.0.1 >nul
if %errorlevel% == 0 (
    echo ✅ Localhost connectivity OK
) else (
    echo ❌ Localhost connectivity failed
)

echo.
echo === Test Results ===
echo If you see ✅ for all tests, the server should be working properly.
echo If you see ❌, check that the server is running and ports are available.
echo.
echo Press any key to continue...
pause >nul
