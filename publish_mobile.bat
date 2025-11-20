@echo off
echo Publishing EncryptItVC Mobile for Android Release...
cd MobileClient

echo.
echo Restoring packages...
dotnet restore

echo.
echo Building Release APK...
dotnet publish -c Release -f net8.0-android

echo.
echo Release build completed!
echo.
echo APK location: MobileClient\bin\Release\net8.0-android\publish\
echo.
echo To install on connected device: 
echo adb install "MobileClient\bin\Release\net8.0-android\publish\com.encryptitvc.mobile-Signed.apk"
echo.
pause
