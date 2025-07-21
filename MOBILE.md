# EncryptItVC Mobile - Android App Guide

## 🎉 NEW: Android Mobile App Available!

EncryptItVC now includes a full-featured Android mobile application built with .NET MAUI!

## ✨ Mobile Features

### 🎙️ Voice Communication
- **Crystal-clear voice chat** with the same 16-bit 44.1kHz quality as desktop
- **Real-time voice status** indicators (muted, deafened, talking)
- **Push-to-talk** and **voice activation** modes
- **Mobile-optimized audio controls**

### 💬 Chat & Channels
- **Real-time text chat** in channels
- **Channel browsing** and joining
- **User list** with admin indicators
- **Private channel** support with passwords

### 📱 Mobile-Optimized UI
- **Dark theme** designed for mobile devices
- **Touch-friendly controls** with large buttons
- **Responsive layout** that works on all screen sizes
- **Navigation drawer** for easy access to features

### ⚙️ Settings & Configuration
- **Audio quality settings** and microphone sensitivity
- **Connection management** with auto-reconnect
- **Voice compression options** for different bandwidth needs

## 🚀 Installation & Setup

### Requirements
- **Android 6.0+** (API level 23+)
- **Microphone permissions** for voice chat
- **Network access** for server connection

### For Developers

#### Prerequisites
1. **Install .NET 8.0+ SDK**
2. **Install MAUI Android Workload:**
   ```bash
   dotnet workload install maui-android
   ```
3. **Android SDK** (installed automatically with workload)

#### Build the App
```bash
# Clone the repository
git clone https://github.com/yourusername/EncryptItVC.git
cd EncryptItVC

# Build Debug APK
.\build_mobile.bat

# Build Release APK
.\publish_mobile.bat
```

#### Install on Device
```bash
# Enable Developer Options and USB Debugging on your Android device
# Connect device via USB

# Install the APK
adb install "MobileClient\bin\Release\net8.0-android\publish\com.encryptitvc.mobile-Signed.apk"
```

### For End Users

1. **Download the APK** from the releases page
2. **Enable "Install from Unknown Sources"** in Android settings
3. **Install the APK** file
4. **Grant microphone permissions** when prompted
5. **Connect to your EncryptItVC server** using the same IP and port

## 📱 Using the Mobile App

### First Launch
1. **Open EncryptItVC Mobile**
2. **Enter server details** (IP:Port, e.g., 192.168.1.100:7777)
3. **Connect** to the server
4. **Login or Register** with your credentials

### Voice Chat
- **Tap and hold** the Push-to-Talk button to speak
- **Use mute/deafen controls** in the bottom toolbar
- **See voice status** indicators next to user names
- **Adjust settings** in the Settings page

### Navigation
- **Swipe from left** or tap menu button to open navigation drawer
- **Switch between** Connection, Voice Chat, Channels, and Settings
- **Tap channels** to join them
- **Send messages** using the chat input

## 🔧 Technical Details

### Architecture
- **Built with .NET MAUI** for native Android performance
- **Shared codebase** with future iOS support planned
- **MVVM pattern** with data binding
- **Async networking** with robust error handling

### Audio System
- **Plugin.Maui.Audio** for cross-platform audio recording/playback
- **Real-time audio streaming** over UDP
- **Compatible with desktop** voice quality standards
- **Mobile-optimized buffering** for reliable performance

### Networking
- **TCP connection** for control messages and chat
- **UDP streaming** for voice data
- **Automatic reconnection** with exponential backoff
- **Connection status monitoring**

## 🛠️ Development Notes

### Project Structure
```
MobileClient/
├── Views/              # XAML pages
│   ├── LoginPage.xaml     # Server connection
│   ├── MainPage.xaml      # Voice chat interface
│   ├── ChannelPage.xaml   # Channel management
│   └── SettingsPage.xaml  # App settings
├── ViewModels/         # MVVM view models
├── Services/           # Business logic
│   ├── ServerConnection.cs    # TCP/UDP networking
│   └── VoiceManager.cs       # Audio recording/playback
├── Models/             # Data models
└── Platforms/Android/  # Android-specific code
```

### Extending the App
- **Add new pages** by creating XAML + ViewModel pairs
- **Extend networking** in ServerConnection.cs
- **Add audio features** in VoiceManager.cs
- **Customize UI** using MAUI controls and styling

## 🚀 Roadmap

### Short Term
- [ ] **iOS version** using the same MAUI codebase
- [ ] **Background voice** support (continue calls when app is minimized)
- [ ] **Push notifications** for mentions and messages
- [ ] **File sharing** through mobile interface

### Long Term
- [ ] **Video calls** with camera support
- [ ] **Screen sharing** from mobile devices
- [ ] **Location sharing** for meetup coordination
- [ ] **Integration** with mobile contact lists

## 🤝 Contributing

The mobile app is part of the main EncryptItVC project. To contribute:

1. **Fork the repository**
2. **Create a feature branch** for mobile improvements
3. **Test on real Android devices**
4. **Submit a pull request** with your changes

## 📞 Support

For mobile app issues:
- **Check logs** in Android logcat
- **Test connectivity** with the server using desktop client first
- **Verify permissions** are granted for microphone and network
- **Report bugs** in the main GitHub repository with "Mobile" label

---

**Enjoy crystal-clear voice communication on the go with EncryptItVC Mobile!** 📱🎙️
