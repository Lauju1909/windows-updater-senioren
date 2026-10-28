# 🛡️ Einfacher & Barrierefreier Computer-Aktualisierer (.EXE)

Eine barrierefreie Windows-Desktop-Anwendung als **echte `.exe`** (keine Konsole), speziell konzipiert für **Seniorinnen, Senioren und blinde Menschen**, um Windows 10 & 11 mit nur einem Tastendruck bzw. Klick vollständig auf den neuesten Stand zu bringen und wichtige Basis-Programme mit einem Klick zu installieren.

GitHub Repository: [https://github.com/Lauju1909/windows-updater-senioren](https://github.com/Lauju1909/windows-updater-senioren)

---

## 🌟 Hauptfunktionen

### 1. 🔄 Alles mit 1 Klick aktualisieren
- **Alle installierten Programme (`winget upgrade`)**:
  - Google Chrome, Mozilla Firefox, Microsoft Edge, VLC, Adobe Acrobat Reader, Zoom usw.
  - Jedes Programm wird einzeln nacheinander aktualisiert – falls ein Programm manuelle Eingaben verlangt, bricht der Vorgang nicht ab, sondern aktualisiert zuverlässig alle anderen Programme weiter!
- **Windows-Sicherheitsupdates**:
  - Sucht, lädt und installiert offizielle Windows-Systemupdates direkt über die Windows Update Schnittstelle (`Microsoft.Update.Session`).
- **Microsoft Store Apps**:
  - Aktualisiert alle Store-Apps (Rechner, Fotos, Windows-Sicherheit, MSN Wetter usw.) über `winget upgrade --all --source msstore`.

---

### 2. ➕ Wichtige Programme installieren (Neuer Knopf!)
Über den Knopf **„➕ PROGRAMME INSTALLIEREN“** (oder Kurzbefehl `Alt + I` / `F3`) öffnet sich eine barrierefreie Auswahlliste mit den wichtigsten Basis-Programmen:
- 👁️ **NVDA**: Der kostenlose, beliebte Bildschirm-Vorleser (Screenreader) für blinde und sehbehinderte Menschen.
- 📝 **LibreOffice**: Kostenloses Office-Paket mit Schreibprogramm (Writer) und Tabellenkalkulation (Calc).
- ✉️ **Mozilla Thunderbird**: Einfaches, übersichtliches E-Mail-Programm.
- 🌐 **Google Chrome**: Schneller und beliebter Webbrowser.
- 🦊 **Mozilla Firefox**: Freier, datenschutzfreundlicher Webbrowser.
- 🚀 **Google Antigravity**: Die Google Antigravity Entwicklungsumgebung.

Einfach Haken setzen und auf **„Ausgewählte jetzt installieren“** klicken – den Rest erledigt der PC vollautomatisch!

---

### 3. 🔄 Automatischer Selbst-Aktualisierer (Auto-Updater)
- Die App prüft beim Starten automatisch im Hintergrund über GitHub (`version.json`), ob ein Update der App vorliegt.
- Bei einem neuen Update lädt sie die neue Version herunter, ersetzt sich selbst und startet neu – ganz ohne Zutun der älteren Person!
- Manuelle Prüfung jederzeit über den Knopf **„Update prüfen“** oder Taste `F5`.

---

## ♿ Barrierefreiheit für blinde und sehbehinderte Personen

- **Integrierte deutsche Sprachausgabe (TTS):**
  - Liest Begrüßung, Tastenkürzel und jeden Schritt live auf Deutsch vor.
- **Tastatur-Kurzbefehle (Hotkeys):**
  - `Enter` oder `Leertaste`: Alles aktualisieren (bzw. Schließen nach Fertigstellung)
  - `Alt + I` oder `F3`: Fenster „Programme installieren“ öffnen
  - `F1`: Aktuellen Status jederzeit laut vorlesen lassen
  - `F5` oder `Alt + U`: Auf App-Updates prüfen
  - `Alt + S`: Sprachausgabe ein-/ausschalten
  - `Alt + N`: Automatischer Neustart umschalten
  - `Alt + D` oder `F2`: Technische Details für Helfer ein-/ausblenden
  - `Esc`: Fenster schließen
- **Screenreader-Kompatibilität:**
  - Optimiert für **NVDA**, **JAWS** und die **Windows-Sprachausgabe (Narrator)**.
- **Echte `.exe` ohne Konsole:**
  - Keine verwirrenden schwarzen Eingabeaufforderungsfenster.

---

## 🚀 Schnellstart

1. **Desktop-Verknüpfung einrichten:**
   - Einmalig doppelklicken auf: [**`Desktop-Verknuepfung-Erstellen.bat`**](file:///C:/Users/lauri/.gemini/antigravity/scratch/windows-updater-senioren/Desktop-Verknuepfung-Erstellen.bat).
   - Auf dem Desktop erscheint das Symbol **„Computer Aktualisieren“** mit dem Schutzschild.
2. **Bedienung:**
   - Doppelklick auf das Symbol (Windows-Abfrage mit **„Ja“** bestätigen).
   - Drücken Sie **`Enter`** zum Aktualisieren aller Programme & Windows.
   - Drücken Sie **`Alt + I`** zum Installieren neuer Programme (NVDA, LibreOffice etc.).
