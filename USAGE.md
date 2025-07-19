# EncryptItVC Használati Útmutató

## 🚀 Első lépések

### 1. Szerver indítása
```bash
# Windows
cd Server
dotnet run

# Linux/macOS  
cd Server
dotnet run
```
A szerver alapértelmezetten a `0.0.0.0:7777` címen indul el.

### 2. Kliens indítása
```bash
# Windows
.\Client\bin\Debug\net6.0-windows\EncryptItVC.Client.exe
```

### 3. Kapcsolódás
1. Add meg a szerver címét: `localhost:7777` (helyi szerver esetén)
2. Regisztrálj vagy jelentkezz be
3. Válaszd ki a csatornát

## 🎮 Felhasználói Felület

### Főablak elrendezése
```
┌─────────────────────────────────────┐
│ EncryptItVC - [Connected/Disconnected] │
├─────────┬───────────────────────────┤
│ Channels│ Chat Messages             │
│ 🏠 Lobby │ User1: Hello!             │
│ 📁 Work  │ User2: How are you?       │
│ 🔒 Private│                          │
├─────────┼───────────────────────────┤
│ Users   │ [Type message here...]    │
│ 👑 Admin │ [Send]                    │
│ 👤 User1 │                          │
│ 🔇 User2 │                          │
├─────────┴───────────────────────────┤
│ [🎤] [🔇] [🔊] [⚙️] [📞]          │
└─────────────────────────────────────┘
```

### Státusz ikonok jelentése
- **👑**: Admin felhasználó
- **🏗️**: Csatornalérehozási jog
- **👤**: Normál felhasználó
- **🔇**: Némítva (muted)
- **🔊**: Süketítve (deafened)
- **🟢**: Online
- **🟡**: Away
- **🔴**: Offline

## 🎙️ Hang Funkciók

### Mikrofon használat
**Push-to-Talk mód:**
1. Tartsd nyomva a mikrofon gombot (🎤)
2. Beszélj a mikrofonba
3. Engedd el a gombot

**Folyamatos mód:**
1. Kattints a mikrofon gombra (🎤) a bekapcsoláshoz
2. A mikrofon folyamatosan rögzít
3. Kattints újra a kikapcsoláshoz

### Hang kontrollok
- **🎤 Mikrofon**: Be/kikapcsolás
- **🔇 Mute**: Saját mikrofon némítása
- **🔊 Deafen**: Minden hang némítása (süketítés)
- **⚙️ Beállítások**: Audio eszközök és hangerő

### Audio minőség beállítások
**Aktuális specifikáció:**
- **Sample Rate**: 44.1 kHz (CD minőség)
- **Bit Depth**: 16-bit
- **Codec**: Tömörítés nélküli PCM
- **Latency**: 70ms end-to-end
- **Noise Gate**: Intelligens zajszűrés

**Optimális beállítások:**
1. Mikrofonot 15-30 cm távolságra tartsd
2. Zárt fejhallgatót használj az echo elkerülésére
3. Csendes környezetet válassz
4. USB mikrofonok általában jobb minőséget adnak

## 💬 Chat Rendszer

### Üzenet küldés
1. **Szöveg bevitel**: Írj a chat mezőbe
2. **Enter vagy Send**: Üzenet elküldése  
3. **Enter+Shift**: Új sor hozzáadása

### Chat parancsok
```
/help                 - Súgó megjelenítése
/me [üzenet]         - Akció üzenet (/me sétál)
/whisper [user] [msg] - Privát üzenet
/clear               - Chat törlése (csak helyi)
```

### Formázási lehetőségek
- **Emoji**: Támogatott 😀 🎉 👍
- **Unicode**: Ékezetes karakterek (éáűőüöíó)
- **Hossz limit**: Maximum 500 karakter/üzenet
- **Link felismerés**: URL-ek automatikus linkké alakítása

## 🏠 Csatorna Kezelés

### Csatornák típusai
**🏠 Nyilvános csatornák:**
- Mindenki csatlakozhat
- Látható a csatorna listában
- Nincs jelszó védelem

**🔒 Privát csatornák:**
- Jelszó szükséges a belépéshez
- Korlátozott láthatóság
- Tulajdonos vezérelt

**🎯 Ideiglenes csatornák:**
- Automatikus törlés ha üres
- Dinamikus létrehozás
- Gyors beszélgetésekhez

### Csatorna navigáció
1. **Csatorna váltás**: Kattints a csatorna nevére
2. **Csatorna info**: Jobb klik → Információ
3. **Kilépés**: Jobb klik → Kilépés
4. **Kedvencek**: Jobb klik → Kedvencekhez adás

### Csatorna létrehozás (jogosultság szükséges)
1. Jobb klik a csatorna listán
2. "Új csatorna létrehozása"
3. Add meg a nevet és beállításokat
4. Válaszd a típust (nyilvános/privát)
5. Állítsd be a jelszót (privát csatornához)

## 👑 Admin Funkciók

### Felhasználó kezelés
**Jogosultságok kiosztása:**
1. Jobb klik a felhasználón
2. "Jogosultságok" menü
3. Válaszd ki a jogosultságot:
   - Channel Creator
   - Admin
   - Ban/Kick jogok

**Moderációs funkciók:**
- **Kick**: Felhasználó kizárása
- **Ban**: Felhasználó tiltása
- **Mute**: Felhasználó némítása
- **Move**: Felhasználó áthelyezése másik csatornába

### Szerver beállítások
**Config szerkesztés (szerver újraindítás szükséges):**
```yaml
server:
  name: "Saját Szerver"
  max_connections: 50
  welcome_message: "Üdvözöllek!"

channels:
  default_channel: "Lobby"
  auto_create_temp: true

security:
  password_min_length: 6
  ban_duration_default: 3600  # másodperc
```

## ⚙️ Beállítások és Testreszabás

### Audio beállítások
**Input Device:**
- Mikrofon kiválasztása
- Input hangerő (0-100%)
- Noise gate szint
- Push-to-talk billentyű

**Output Device:**
- Hangszóró/fejhallgató kiválasztása
- Master hangerő (0-100%)
- Per-user hangerő beállítás
- Audio minőség (sample rate)

### Interface beállítások
**Témák:**
- Dark theme (alapértelmezett)
- Light theme
- High contrast
- Egyéni színsémák

**Nyelv:**
- Magyar (alapértelmezett)
- English
- Deutsch
- További nyelvek...

### Notification beállítások
- **Új üzenet**: Hang + popup
- **Felhasználó csatlakozás**: Csak hang
- **Csatorna változás**: Kikapcsolva
- **Rendszer üzenetek**: Popup

## 🔧 Speciális Funkciók

### Hotkey-ek (gyorsbillentyűk)
```
Ctrl+M          - Mute toggle
Ctrl+D          - Deafen toggle
Ctrl+T          - Push-to-talk (press & hold)
F5              - Refresh channel list
Ctrl+Enter      - Send message
Esc             - Cancel current action
```

### Multi-server kapcsolat
**Több szerver kezelése:**
1. File → Új kapcsolat
2. Add meg a szerver adatokat
3. Válts a kapcsolatok között (Tab fülekkel)
4. Mentett szerverek listája

### Voice aktiválás (VOX)
**Automatikus mikrofonkapcsolás:**
1. Beállítások → Voice Activation
2. Threshold beállítása (érzékenység)
3. Hold time (mennyi ideig maradjon aktív)
4. Release time (mikor kapcsoljon ki)

### Overlay mód
**In-game overlay:**
- Minimális felület játék közben
- Csak essential kontrolok
- Átlátszó háttér
- Always on top

## 📊 Teljesítmény Monitoring

### Kapcsolat státusz
**Hálózati információk:**
- Ping idő
- Packet loss
- Bandwidth használat
- Connection quality

**Audio teljesítmény:**
- Input/Output latency
- Sample rate
- Bit rate
- Buffer használat

### Hibakeresés és diagnosztika
**Debug információk:**
- Console log megtekintése
- Network traffic monitoring
- Audio device status
- Memory/CPU usage

## 📱 Mobil és Távoli Hozzáférés

### Távoli szerver hozzáférés
**Internet kapcsolaton keresztül:**
1. Router port forwarding (7777, 7778)
2. External IP cím használata
3. Dynamic DNS beállítás (opcionális)
4. VPN kapcsolat (ajánlott)

### Biztonság távoli hozzáféréshez
- **Erős jelszavak**: Minimum 12 karakter
- **Whitelist**: Csak engedélyezett IP-k
- **Rate limiting**: Brute force védelem
- **SSL/TLS**: Titkosított kapcsolat (jövőbeni funkció)

## 🆘 Gyors Segítség

### Gyakori problémák megoldása
**Nem hallom a hangot:**
1. Ellenőrizd az output eszközt
2. Hangerő beállítások
3. Deafen kikapcsolása
4. Audio driver frissítés

**Nem hall a mikrofonomat:**
1. Input eszköz ellenőrzés
2. Mikrofon engedélyek (Windows)
3. Mute kikapcsolása
4. Hardware kapcsolat

**Kapcsolódási problémák:**
1. Szerver fut és elérhető
2. IP cím és port helyes
3. Tűzfal/antivirus engedélyek
4. Internet kapcsolat stabil

### Támogatás elérése
- **F1**: Beépített súgó
- **GitHub Issues**: https://github.com/user/EncryptItVC/issues
- **Discord**: [Meghívó link]
- **Email**: support@encryptitvc.com

---

**💡 Tipp**: Kezdd a Local Area Network (LAN) teszteléssel, mielőtt internet kapcsolaton próbálnád!
