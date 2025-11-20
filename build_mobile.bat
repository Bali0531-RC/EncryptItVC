@echo off
echo Building EncryptItVC Mobile for Android...
cd MobileClient

echo.
echo Restoring packages...
dotnet restore

echo.
echo Building Debug APK...
dotnet build -c Debug -f net8.0-android

echo.
echo Build completed! 
echo Debug APK location: MobileClient\bin\Debug\net8.0-android\
echo.

echo To install on device: dotnet run -f net8.0-android
echo To build Release APK: dotnet publish -c Release -f net8.0-android
echo.
pause
