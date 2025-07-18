# EncryptItVC - Telepítési és Használati Útmutató

## 🚀 Gyors Start

### Windows

#### Szükséges szoftverek telepítése:

1. **Töltsd le és telepítsd a .NET SDK-t (6.0 vagy újabb):**
   - Látogasd meg: https://dotnet.microsoft.com/download/dotnet
   - Töltsd le a legfrissebb ".NET SDK" verziót (.NET 6, 7, 8, 9 mind jó)
   - Telepítsd és indítsd újra a számítógépet

2. **Nyisd meg a terminált** (Command Prompt vagy PowerShell)

#### Szerver indítása:

```cmd
cd a:\EncryptItVC\Server
dotnet restore
dotnet run
```

### Linux/macOS

#### .NET telepítése:

**Automatikus ellenőrzés és telepítés:**
```bash
chmod +x install_dotnet.sh
./install_dotnet.sh
```

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install dotnet-runtime-6.0
```

**CentOS/RHEL/Fedora:**
```bash
sudo dnf install dotnet-runtime-6.0
```

**macOS (Homebrew):**
```bash
brew install dotnet
```

**Megjegyzés:** Ha már van .NET 6+ verziód telepítve (.NET 7, 8, 9), akkor nem kell semmit csinálni!

#### Szerver indítása:

```bash
chmod +x start_server.sh
./start_server.sh
```

**Vagy manuálisan:**
```bash
cd Server
dotnet restore
dotnet run
```

A szerver automatikusan elindul és várja a kapcsolatokat a 7777-es porton.

### Kliens buildélése:

```cmd
cd a:\EncryptItVC\Client
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true
```

A kész .exe fájl itt található: `Client\bin\Release\net6.0-windows\win-x64\publish\EncryptItVC.Client.exe`

### Alternatíva - Script fájlok használata:

**Windows:**
Egyszerűen dupla kattintás a gyökérkönyvtárban:
- `build_server.bat` - Szerver build
- `build_client.bat` - Kliens build  
- `publish_client.bat` - Kliens .exe készítése
- `start_server.bat` - Szerver indítása

**Linux/macOS:**
Terminálból futtatható scriptek:
```bash
chmod +x *.sh
./build_server.sh    # Szerver build
./start_server.sh    # Szerver indítása
./install_dotnet.sh  # .NET telepítése
```

## 🎯 Első használat

1. **Indítsd el a szervert** a `start_server.bat` fájllal vagy manuálisan
2. **Build-eld a klienst** a `publish_client.bat` fájllal
3. **Indítsd el a klienst** és csatlakozz a `127.0.0.1:7777` címre
4. **Regisztrálj** egy új felhasználót vagy jelentkezz be mint **admin/admin123**

## 👑 Admin funkciók

- **Felhasználónév:** admin
- **Jelszó:** admin123 (VÁLTOZTASD MEG a config.yml-ben!)
- **Képességek:** Csatornák létrehozása, jogosultságok kiosztása

## 📋 Funkciók

✅ **Működő funkciók:**
- Szerver-kliens kapcsolat
- Felhasználói regisztráció/bejelentkezés
- Csatornák létrehozása és csatlakozás
- Valós idejű chat
- Admin jogosultságkezelés
- Privát csatornák jelszóval
- Modern fekete UI

⚠️ **Hangfunkcióhoz szükséges:**
- A hangfunkció alapjai be vannak építve
- További finomhangolás szükséges a teljes funkcionalitáshoz

## 🔧 Konfiguráció

A `Server/config.yml` fájlban tudod módosítani:
- Admin jelszó (FONTOS!)
- Szerver port
- Maximális kapcsolatok száma
- Titkosítási kulcs

## 🛡️ Biztonság

- Minden jelszó hash-elve van tárolva
- Üzenetek titkosítva vannak
- Csatorna-szintű hozzáférés-szabályozás
- Admin jogosultságok külön kezelése

## 🤝 Támogatás

Ha problémáid vannak:
1. **Windows:** Ellenőrizd, hogy a .NET 6.0 SDK telepítve van
2. **Linux:** Ellenőrizd, hogy a .NET 6.0 Runtime telepítve van (`dotnet --version`)
3. Győződj meg róla, hogy a 7777-es port szabad (`netstat -tlnp | grep :7777`)
4. Nézd meg a szerver log üzeneteit
5. Ellenőrizd a tűzfal beállításokat:
   - **Windows:** Windows Defender Firewall
   - **Linux:** `sudo ufw allow 7777 && sudo ufw allow 7778`
   - **macOS:** System Preferences > Security & Privacy > Firewall

**Jó kommunikációt!** 🎙️
