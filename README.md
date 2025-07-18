# EncryptItVC - Secure Voice Communication Platform

🎙️ **EncryptItVC** egy TeamSpeak-szerű, titkosított hang- és szövegkommunikációs platform, amely szerver-kliens architektúrával működik.

## 🚀 Jellemzők

- **Biztonságos kommunikáció**: Titkosított hang- és szövegátvitel
- **Csatornaalapú rendszer**: Külön csatornák létrehozása és kezelése
- **Felhasználói jogosultságok**: Admin és csatornalérehozási jogosultságok
- **Valós idejű chat**: Szöveges üzenetküldés csatornákon belül
- **Hangkommunikáció**: Valós idejű hangátvitel (push-to-talk)
- **Modern UI**: Fekete témájú, modern felhasználói felület
- **Egyszerű beállítás**: YAML konfigurációs fájl

## 📋 Rendszerkövetelmények

- **Szerver**: .NET 9.0 vagy újabb (Windows, Linux, macOS) - Cross-platform támogatás
- **Kliens**: Windows 10+ .NET 6.0 Desktop Runtime (csak Windows)
- **Hálózat**: TCP/UDP port hozzáférés

## 🛠️ Telepítés és Használat

### Szerver indítása

#### Windows:
1. Navigálj a `Server` mappába
2. Szerkeszd a `config.yml` fájlt (admin jelszó, portok, stb.)
3. Buildeld és indítsd a szervert:

```bash
cd Server
dotnet build
dotnet run
```

#### Linux/macOS:
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

3. A szerver alapértelmezetten a `0.0.0.0:7777` címen fog futni, ami minden interfészen elérhető.

#### Docker (Ajánlott Linux-hoz):
1. Telepítsd a Docker-t és Docker Compose-t
2. Buildeld és indítsd a szervert:
   ```bash
   chmod +x start_docker.sh
   ./start_docker.sh
   ```

   Vagy manuálisan:
   ```bash
   cd Server
   dotnet build -c Release
   cd ..
   docker-compose up -d
   ```

### Kliens buildelse

1. Navigálj a `Client` mappába
2. Buildeld a kliensalkalmazást:

```bash
cd Client
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true
```

3. Az .exe fájl a `bin/Release/net6.0-windows/win-x64/publish/` mappában található

## ⚙️ Konfiguráció

A szerver konfigurációja a `config.yml` fájlban található:

```yaml
server:
  host: "0.0.0.0"        # Szerver IP címe
  port: 7777             # TCP port
  name: "EncryptItVC Server"

admin:
  username: "admin"      # Admin felhasználónév
  password: "admin123"   # Admin jelszó (VÁLTOZTASD MEG!)

security:
  encryption_key: "your-32-character-secret-key-here"
  max_connections: 100
```

## 👥 Felhasználói Jogosultságok

### Adminisztrátor
- Teljes hozzáférés minden funkcióhoz
- Jogosultságok kiadása más felhasználóknak
- Csatornák létrehozása és kezelése

### Csatornalérehozó
- Saját csatornák létrehozása
- Saját csatornák kezelése
- Privát csatornák jelszavakkal

### Normál felhasználó
- Csatlakozás nyilvános csatornákhoz
- Szöveges és hang kommunikáció
- Csatornák böngészése

## 📖 Használati útmutató

1. **Első indítás**: Indítsd el a klienst és adj meg szerverIP:port-ot
2. **Regisztráció**: Ha nincs még felhasználód, regisztrálj
3. **Bejelentkezés**: Használd a felhasználóneved és jelszavad
4. **Csatorna váltás**: Kattints egy csatornára a listában
5. **Chat**: Írj üzenetet a szövegmezőbe és nyomj Enter-t
6. **Hang**: Tartsd nyomva a mikrofon gombot beszéd közben

## 🔧 Fejlesztés

### Technológiák
- **Backend**: C# .NET 6.0
- **Frontend**: WPF + Material Design
- **Hálózat**: TCP (üzenetek) + UDP (hang)
- **Konfiguráció**: YAML
- **Hang**: NAudio

### Projekt struktúra
```
EncryptItVC/
├── Server/           # Szerver alkalmazás
├── Client/           # Kliens alkalmazás
├── Shared/           # Közös kódkönyvtár
└── README.md         # Dokumentáció
```

## 🔒 Biztonság

- Minden üzenet titkosítva van
- Felhasználói jelszavak hash-elve vannak tárolva
- Csatorna-specifikus jogosultságkezelés
- Biztonságos admin hozzáférés

## 🐛 Hibaelhárítás

### Kapcsolódási problémák
- Ellenőrizd a szerverIP és port beállításokat
- Győződj meg róla, hogy a tűzfal engedélyezi a forgalmat
- Ellenőrizd, hogy a szerver fut-e

### Linux szerver problémák
- Ellenőrizd a tűzfal beállításokat: `sudo ufw allow 7777` és `sudo ufw allow 7778`
- Győződj meg róla, hogy a portok szabadok: `netstat -tlnp | grep :7777`
- Ellenőrizd a .NET telepítését: `dotnet --version`

### Hang problémák
- Ellenőrizd a mikrofon és hangszóró beállításokat
- Győződj meg róla, hogy más alkalmazás nem használja a hangeszközöket

## 📄 Licenc

MIT License - Szabad felhasználásra és módosításra.

## 🤝 Közreműködés

Jelentsd a hibákat és javasold a fejlesztéseket a GitHub repository-ban!

---

**Készítette**: Chorus - Bali0531
**Verzió**: 1.0.0  
**Utolsó frissítés**: 2025-07-18
