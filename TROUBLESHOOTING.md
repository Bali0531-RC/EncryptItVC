# EncryptItVC Hibaelhárítási Útmutató

## 🔧 Gyakori Problémák és Megoldások

### 🔌 Kapcsolódási Problémák

#### "Connection failed" hibaüzenet
**Okok és megoldások:**
- **Szerver nem fut**: Indítsd el a szervert `dotnet run` paranccsal a Server mappában
- **Rossz IP/port**: Ellenőrizd hogy a helyes IP címet és portot adtad-e meg (alapértelmezett: 7777)
- **Tűzfal blokkolja**: Windows Defender vagy antivirus blokkolhatja a kapcsolatot

```bash
# Szerver státusz ellenőrzés
netstat -an | findstr :7777

# Windows tűzfal engedélyezés
netsh advfirewall firewall add rule name="EncryptItVC" dir=in action=allow protocol=TCP localport=7777
netsh advfirewall firewall add rule name="EncryptItVC-UDP" dir=in action=allow protocol=UDP localport=7778
```

#### "Not connected" popup spam
**Megoldva**: Ez a probléma a legújabb verzióban javítva lett intelligens újracsatlakozással.

#### Automatikus újracsatlakozás nem működik
**Ellenőrizd:**
- Stabil internetkapcsolat
- Szerver fut és elérhető
- Nincs tűzfal/proxy akadály

### 🎙️ Audio Problémák

#### Nem hallható hang / akadozó hang
**Megoldva**: A legújabb verzióban implementálva:
- Kristálytiszta 16-bit, 44.1kHz audio
- 70ms ultra-alacsony késleltetés
- Tömörítés nélküli PCM átvitel

#### Mikrofon nem működik
**Ellenőrizd:**
1. **Eszköz kiválasztás**: Audio beállítások → Input device
2. **Windows engedélyek**: Settings → Privacy → Microphone → Desktop apps
3. **Driver frissítés**: Eszközkezelő → Audio devices → Driver update
4. **Exkluzív hozzáférés**: Más alkalmazás használja-e a mikrofont

```bash
# Audio eszközök listázása (PowerShell)
Get-WmiObject -Class Win32_SoundDevice | Select-Object Name, Status
```

#### Hangszóró/fejhallgató nem működik
**Ellenőrizd:**
1. **Output device**: Audio beállítások → Output device
2. **Hangerő**: Windows hangerő és alkalmazás hangerő
3. **Audio driver**: Frissítsd az audio driver-eket
4. **Audio format**: 44.1kHz, 16-bit formátum támogatás

#### Süketítés (Deafen) nem működik
**Megoldva**: A legújabb verzióban javítva a deafen logika.

### 👤 Felhasználói Problémák

#### Nem tudok regisztrálni
**Ellenőrizd:**
- Egyedi felhasználónév (nem foglalt)
- Jelszó minimum követelmények
- Szerver elérhető és fut

#### Elfelejtett jelszó
**Admin megoldás:**
1. Állítsd le a szervert
2. Szerkeszd a felhasználói adatbázist
3. Indítsd újra a szervert

#### Admin jogosultságok
**Admin felhasználó alapértelmezett adatai:**
- Username: `admin`
- Password: `admin123` (VÁLTOZTASD MEG PRODUCTION-BAN!)

### 🏠 Csatorna Problémák

#### Nem látom a csatornákat
**Megoldások:**
- Frissítsd a csatorna listát (F5 vagy reconnect)
- Ellenőrizd a jogosultságaidat
- Szerver újraindítása

#### Nem tudok csatornát létrehozni
**Ellenőrizd:**
- Van-e "Create Channel" jogosultságod
- Egyedi csatorna név
- Szerver limit ellenőrzés

#### Privát csatorna jelszó
**Ha elfelejtetted:**
- Admin törölheti és újra létrehozhatja
- Jelszó visszaállítás csak admin jogokkal

### 💬 Chat Problémák

#### Üzenetek nem jelennek meg
**Megoldva**: A chat rendszer teljesen javítva a legújabb verzióban.

#### Üzenetek formázási problémák
**Karakterek támogatása:**
- UTF-8 encoding támogatott
- Emoji-k támogatottak
- Speciális karakterek (éáűőüöíó)

### 🖥️ Szerver Problémák

#### Szerver nem indul
**Ellenőrizd:**
1. **.NET verzió**: `dotnet --version` (9.0+ szükséges)
2. **Config fájl**: `config.yml` helyes formátumban
3. **Port foglaltság**: `netstat -an | findstr :7777`

```bash
# Config fájl validálás
cd Server
dotnet run --configuration Debug
```

#### Magas CPU/memória használat
**Optimalizálás:**
- Limit a max_connections értékét
- Restart szerver rendszeresen
- Monitoring implementálás

#### Port már használatban
```bash
# Port használat ellenőrzés
netstat -ano | findstr :7777

# Process kill (ha szükséges)
taskkill /PID <PID> /F
```

### 🐧 Linux/macOS Specifikus Problémák

#### .NET telepítés Ubuntu/Debian
```bash
# Microsoft package repository hozzáadása
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# .NET 9.0 telepítés
sudo apt update
sudo apt install dotnet-runtime-9.0
```

#### .NET telepítés CentOS/RHEL/Fedora
```bash
# .NET repository hozzáadása
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm

# .NET 9.0 telepítés
sudo dnf install dotnet-runtime-9.0
```

#### Tűzfal konfiguráció Linux
```bash
# UFW (Ubuntu)
sudo ufw allow 7777/tcp
sudo ufw allow 7778/udp

# Firewalld (CentOS/RHEL)
sudo firewall-cmd --permanent --add-port=7777/tcp
sudo firewall-cmd --permanent --add-port=7778/udp
sudo firewall-cmd --reload

# iptables (általános)
sudo iptables -A INPUT -p tcp --dport 7777 -j ACCEPT
sudo iptables -A INPUT -p udp --dport 7778 -j ACCEPT
```

#### Permissions Linux
```bash
# Szerver futtatási jogosultság
chmod +x start_server.sh

# Config fájl olvasási jog
chmod 644 config.yml

# Log fájlok írási jog
chmod 755 logs/
```

## 🔍 Debugging és Diagnosztika

### Log Fájlok
**Szerver logok:**
- Console output tartalmazza a részletes információkat
- Connection események naplózva
- Error üzenetek traceback-kel

**Kliens logok:**
- Windows Event Log
- Application console output
- Exception details

### Hálózati Diagnosztika

#### Kapcsolat teszt
```bash
# TCP kapcsolat teszt
telnet <server_ip> 7777

# UDP kapcsolat teszt (PowerShell)
Test-NetConnection -ComputerName <server_ip> -Port 7777

# Ping teszt
ping <server_ip>
```

#### Traceroute hálózati útvonal
```bash
# Windows
tracert <server_ip>

# Linux/macOS
traceroute <server_ip>
```

### Performance Monitoring

#### Szerver teljesítmény
```bash
# CPU és memória monitoring
top -p $(pgrep -f "EncryptItVC.Server")

# Hálózati forgalom
netstat -i
```

#### Audio latency teszt
- Mikrofon → Hangszóró teljes késleltetés mérése
- Hálózati ping + audio processing time
- Optimális: <100ms, jelenlegi: ~70ms

## 📞 Támogatás Kérése

### Információk gyűjtése hiba jelentéshez:
1. **Verzió**: Kliens és szerver verzió
2. **Operációs rendszer**: Windows/Linux/macOS verzió
3. **Hálózat**: Helyi hálózat vagy internet
4. **Log üzenetek**: Console output másolása
5. **Reprodukálási lépések**: Hogyan idézhető elő a hiba

### Kapcsolat:
- **GitHub Issues**: Részletes hibajegyek
- **Discord**: Gyors segítség
- **Email**: Privát problémák

## ✅ Preventív Karbantartás

### Rendszeres feladatok:
- [ ] **Config backup**: `config.yml` mentése
- [ ] **Log tisztítás**: Régi log fájlok törlése
- [ ] **Update check**: Új verziók ellenőrzése
- [ ] **Security audit**: Jelszavak és jogosultságok felülvizsgálata
- [ ] **Performance monitoring**: Szerver erőforrás használat

### Javasolt beállítások production környezetben:
- [ ] Admin jelszó megváltoztatása
- [ ] Tűzfal szabályok beállítása
- [ ] SSL/TLS tanúsítvány (jövőbeni funkció)
- [ ] Backup stratégia
- [ ] Monitoring és alerting
