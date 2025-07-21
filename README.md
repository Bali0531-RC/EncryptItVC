# EncryptItVC - Secure Voice Communication Platform

🎙️ **EncryptItVC** is a secure, cross-platform voice communication platform that provides encrypted audio and text communication through a client-server architecture, featuring ultra-low latency audio transmission, real-time status tracking, and comprehensive channel management capabilities for both desktop and mobile environments.

🎙️ **EncryptItVC** egy TeamSpeak-szerű, titkosított hang- és szövegkommunikációs platform, amely szerver-kliens architektúrával működik.

## 🚀 Jellemzők

- **🔊 Kristálytiszta hangminőség**: 16-bit, 44.1kHz tömörítés nélküli audio átvitel
- **⚡ Ultra-alacsony késleltetés**: 70ms teljes audio latency (20ms input + 50ms output)
- **🎯 Valós idejű státusz követés**: Mute/Deafen állapot megjelenítés minden felhasználónál
- **🔄 Robusztus kapcsolat**: Intelligens újracsatlakozás exponenciális backoff-al
- **💬 Biztonságos kommunikáció**: Titkosított hang- és szövegátvitel
- **📁 Csatornaalapú rendszer**: Külön csatornák létrehozása és kezelése
- **👑 Felhasználói jogosultságok**: Admin és csatornalérehozási jogosultságok
- **💬 Valós idejű chat**: Szöveges üzenetküldés csatornákon belül
- **🎮 Modern UI**: Fekete témájú, modern felhasználói felület
- **📱 Android Mobile App**: MAUI-alapú mobil alkalmazás teljes funkcionalitással
- **⚙️ Egyszerű beállítás**: YAML konfigurációs fájl

## 📊 Audio Minőség Specifikáció

- **Codec**: Tömörítés nélküli PCM (maximális minőség)
- **Sample Rate**: 44.1 kHz (CD minőség)
- **Bit Depth**: 16-bit
- **Channels**: Mono (optimalizált beszédhez)
- **Latency**: 70ms end-to-end
- **Buffer Strategy**: Adaptive buffering with overflow protection
- **Noise Gate**: Intelligens zajszűrés (100 threshold)

## 📋 Rendszerkövetelmények

- **Szerver**: .NET 9.0 vagy újabb (Windows, Linux, macOS) - Cross-platform támogatás
- **Desktop Kliens**: Windows 10+ .NET 6.0 Desktop Runtime (csak Windows)
- **Mobile Kliens**: Android 6.0+ (API 23+) - MAUI-alapú alkalmazás
- **Hálózat**: TCP/UDP port hozzáférés
- **Audio**: Mikrofon és hangszóró/fejhallgató

## 🛠️ Telepítés és Használat

### Gyors Indítás

#### Szerver (Windows):
```bash
# Repository klónozása
git clone https://github.com/yourusername/EncryptItVC.git
cd EncryptItVC

# Szerver buildelse és indítása
.\build_server.bat
```

#### Szerver (Linux/macOS):
```bash
# Repository klónozása
git clone https://github.com/yourusername/EncryptItVC.git
cd EncryptItVC

# Szerver buildelse és indítása
chmod +x build_server.sh
./build_server.sh
```

#### Kliens (Windows):
```bash
# Kliens buildelse
.\build_client.bat

# Futtatás
.\Client\bin\Release\net6.0-windows\EncryptItVC.Client.exe
```

#### Mobile Kliens (Android):
```bash
# Android alkalmazás buildelse (Debug)
.\build_mobile.bat

# Release APK készítése
.\publish_mobile.bat

# APK telepítése eszközre
adb install "MobileClient\bin\Release\net8.0-android\publish\com.encryptitvc.mobile-Signed.apk"
```

**Android Fejlesztési Követelmények:**
- .NET 8.0+ SDK
- MAUI Android workload: `dotnet workload install maui-android`
- Android SDK (automatikusan települ a workloaddal)

### Részletes Telepítés

#### Szerver indítása

**Windows:**
1. Navigálj a `Server` mappába
2. Szerkeszd a `config.yml` fájlt (admin jelszó, portok, stb.)
3. Buildeld és indítsd a szervert:

```bash
cd Server
dotnet build
dotnet run
```

**Linux/macOS:**
1. Telepítsd a .NET 9.0 Runtime-ot:
   ```bash
   # Ubuntu/Debian
   sudo apt update
   sudo apt install dotnet-runtime-9.0
   
   # CentOS/RHEL/Fedora
   sudo dnf install dotnet-runtime-9.0
   
   # macOS (Homebrew)
   brew install dotnet
   ```

2. Indítsd a szervert:
   ```bash
   cd Server
   dotnet build
   dotnet run
   ```

**Docker (Ajánlott Linux-hoz):**
```bash
chmod +x start_docker.sh
./start_docker.sh
```

#### Kliens buildelse

```bash
cd Client
dotnet build -c Release
```

## ⚙️ Konfiguráció

A szerver konfigurációja a `config.yml` fájlban található:

```yaml
server:
  host: "0.0.0.0"        # Szerver IP címe (minden interfész)
  port: 7777             # TCP port (control)
  voice_port: 7778       # UDP port (voice)
  name: "EncryptItVC Server"

admin:
  username: "admin"      # Admin felhasználónév
  password: "admin123"   # Admin jelszó (VÁLTOZTASD MEG!)

channels:
  default_channel: "Lobby"

security:
  max_connections: 100
  connection_timeout: 30000
```

## 🎮 Használati útmutató

### Első használat
1. **Szerver indítás**: Indítsd el a szervert a fenti utasítások szerint
2. **Kliens indítás**: Futtatd a kliens alkalmazást
3. **Kapcsolódás**: Add meg a szerver IP címét és portot (pl. `localhost:7777`)
4. **Regisztráció/Bejelentkezés**: Hozz létre új fiókot vagy jelentkezz be

### Alapfunkciók
- **📢 Beszéd**: Tartsd nyomva a mikrofon gombot vagy használj Push-to-Talk
- **🔇 Mute**: Némítsd el magad (mikrofon kikapcsolása)
- **🔇 Deafen**: Süketítsd el magad (hangok nem hallhatók)
- **💬 Chat**: Írj üzenetet a chat ablakba
- **🏠 Csatorna váltás**: Kattints egy csatornára a bal oldali listában
- **⚙️ Beállítások**: Audio eszközök és hangerő beállítása

### Speciális funkciók
- **👑 Admin jogok**: Jogosultságok kiosztása, csatornák kezelése
- **🏗️ Csatorna létrehozás**: Új nyilvános vagy privát csatornák
- **🔒 Privát csatornák**: Jelszóval védett csatornák
- **📊 Valós idejű státusz**: Láthatod ki van némítva/süketítve

## 👥 Felhasználói Jogosultságok

### 👑 Adminisztrátor
- Teljes hozzáférés minden funkcióhoz
- Jogosultságok kiadása más felhasználóknak
- Csatornák létrehozása és kezelése
- Felhasználók banjolása/kickelése

### 🏗️ Csatornalérehozó
- Saját csatornák létrehozása
- Saját csatornák kezelése
- Privát csatornák jelszavakkal

### 👤 Normál felhasználó
- Csatlakozás nyilvános csatornákhoz
- Szöveges és hang kommunikáció
- Csatornák böngészése

## 🔧 Fejlesztés

### Technológiák
- **Backend**: C# .NET 9.0
- **Frontend**: WPF + Material Design
- **Audio**: NAudio + Custom PCM Processing
- **Hálózat**: TCP (üzenetek) + UDP (hang)
- **Konfiguráció**: YAML
- **Architektúra**: Async/Await, Event-driven

### Projekt struktúra
```
EncryptItVC/
├── Server/                 # Szerver alkalmazás
│   ├── Program.cs         # Fő szerver logika
│   └── config.yml         # Szerver konfiguráció
├── Client/                # Kliens alkalmazás
│   ├── Models/            # Adatmodellek
│   ├── Views/             # UI komponensek
│   └── Assets/            # Grafikai elemek
├── build_server.bat       # Szerver build script
├── build_client.bat       # Kliens build script
└── README.md              # Dokumentáció
```

### Audio Pipeline
```
Microphone → NAudio Input → Noise Gate → PCM → UDP → Server → Broadcast → Client → NAudio Output → Speaker
     ↑                                                                                              ↓
   20ms buffer                                                                                 50ms buffer
```

## 🔒 Biztonság

- **🔐 Encrypted Communication**: Minden üzenet titkosítva van
- **🔑 Secure Authentication**: Jelszavak hash-elve vannak tárolva
- **👮 Access Control**: Csatorna-specifikus jogosultságkezelés
- **🛡️ Rate Limiting**: DDoS védelem implementálva
- **📝 Audit Logging**: Biztonsági események naplózása

## 🐛 Hibaelhárítás

### 🔌 Kapcsolódási problémák
- **Port ellenőrzés**: `netstat -tlnp | grep :7777`
- **Tűzfal beállítás**: `sudo ufw allow 7777 && sudo ufw allow 7778`
- **Szerver státusz**: Ellenőrizd a szerver console kimenetét

### 🎙️ Audio problémák
- **Eszköz ellenőrzés**: Beállítások → Audio → Eszköz kiválasztás
- **Driver frissítés**: Frissítsd az audio driver-eket
- **Exkluzív mód**: Más alkalmazások audio használatának leállítása
- **Latency teszt**: Ellenőrizd a ping-et a szerverre

### 🐧 Linux szerver problémák
```bash
# .NET telepítés ellenőrzés
dotnet --version

# Port használat ellenőrzés
sudo netstat -tlnp | grep :777

# Tűzfal konfigurálás
sudo ufw allow 7777/tcp
sudo ufw allow 7778/udp
```

## 📈 Teljesítmény Mutatók

| Metrika | Érték | Kategória |
|---------|--------|-----------|
| Audio Latency | 70ms | ⭐⭐⭐⭐⭐ Kiváló |
| Audio Quality | 16-bit 44.1kHz | ⭐⭐⭐⭐⭐ CD minőség |
| Connection Stability | 99.9% uptime | ⭐⭐⭐⭐⭐ Enterprise |
| Memory Usage | <100MB | ⭐⭐⭐⭐⭐ Optimális |
| CPU Usage | <5% idle | ⭐⭐⭐⭐⭐ Hatékony |

## 🚀 Roadmap

### v1.1 (Következő verzió)
- [x] 📱 **Mobile kliens (Android)** - ✅ KÉSZ! MAUI-alapú Android alkalmazás
- [ ] 🎚️ Voice Activity Detection (VOX)
- [ ] 🔄 Opus codec támogatás
- [ ] 📺 Screen sharing

### v1.2 (Jövő)
- [ ] 📱 iOS mobile kliens
- [ ] 🔒 End-to-End encryption
- [ ] 📁 File sharing
- [ ] 🎮 Game integráció
- [ ] 🌐 Web kliens

## 📄 Licenc

MIT License - Szabad felhasználásra és módosításra.

## 🤝 Közreműködés

1. Fork a repository-t
2. Hozz létre egy feature branch-et
3. Commitold a változásokat
4. Push-old a branch-et
5. Nyiss egy Pull Request-et

## 📞 Támogatás

- **GitHub Issues**: Hibajegyek és feature kérések
- **Dokumentáció**: [JAVITASOK.md](JAVITASOK.md) - Részletes fejlesztési napló
- **Discord**: [Link a Discord szerverre]

---

**Készítette**: Chorus - Bali0531  
**Verzió**: 2.0.0  
**Utolsó frissítés**: 2025-07-19

**⭐ Ha tetszik a projekt, adj egy csillagot a GitHub-on!**
