# Konzept Architektur – plan.md (Entwurf)

Stand: 2026-10-22 · Status: **Entwurf, nur Abschnitt 1 ist entschieden** (dort stehen auch die Stack-Entscheidungen).
Quellen: Kundenauftrag „Datenbanktrainer“ (BSU Akademie für IT-Berufe), Importformat-Vorgabe des Kunden (siehe `Draft Importformat.md`).

## 1. Entschieden (vom Menschen)

- Repo-Struktur `docs/`, `src/`, `tests/` (kein `logs/`), Solution `sql-trainer-app.slnx`
- Branch-Regeln: `main` geschützt, `feature/*` und `bugfix/*`, nur PR mit 1 Review, Squash
- SQL-Engine/Dialekt: **SQLite eingebettet**; `datenbank.sql` der Kunden muss SQLite-Dialekt sein (siehe 3.1). Dem Kunden vor Beginn der Inhaltserstellung mitzuteilen
- UI-Muster: **leichtes MVVM** (siehe 3.2a)
- UI-Technologie: **Blazor (Interactive Server) mit MudBlazor**; Hülle = **lokaler ASP.NET-Core-Webserver + Browser**, wie das Template `SqlApp` auf `main` (PR #1: Blazor Web App mit MudBlazor, Single-File-Export). Entscheidung vom 2026-10-22; sie ersetzt „WPF + BlazorWebView, nur Windows“. Grund: Mitentwickler arbeiten auch unter Linux, WPF und MAUI laufen dort nicht (siehe 3.2)
- Schichten (2026-10-29): Core / Import / Execution / Storage / **Application** / UI (Razor Class Library) / App. Die Application-Schicht ist neu und enthält die Use Cases (Import, Prüfung, Löschen, Aufgabe bearbeiten); Projektnamen bleiben `SqlTrainer.*`
- Das Solution-Skelett (Etappe 1) wird auf dem Branch `feature/init` gebaut, nicht auf `feature/solution-skeleton` (siehe 5)

Alles Weitere unten sind **Optionen mit Empfehlung**.

## 2. Scope (aus dem Kundenauftrag)

**Nutzer:** Lernende (Azubis FI, Studierende) lösen Aufgaben; Lehrkräfte/Admins verwalten Datenbanken.

**Muss (v1):**
- Desktop-App, lauffähig auf aktuellen Desktop-Systemen, intuitive UI, dauerhafte Speicherung (Umsetzung als lokal gestarteter Host + Browser, siehe 3.2 und Risiko 6)
- Ablauf: Übungs-DB wählen (Name, Beschreibung, ER-Diagramm) → Themenbereich → Aufgabe → SQL-Editor. Hilfehinweis und Aufgabentext jederzeit abrufbar
- Lösung prüfen (richtig/falsch), Musterlösung optional anzeigen
- Verwaltung: DBs anzeigen, per ZIP importieren, löschen (mit Bestätigung)
- Aufgaben dauerhaft gespeichert, bearbeitbar, neue Aufgaben anlegbar
- Erweiterung ohne Quellcodeänderung (Import)
- Auslieferung: 5 Übungs-DBs × 25 Aufgaben = 125 Aufgaben mit Hilfen und Musterlösungen
- Lieferumfang außerhalb der App: Importformat-Spezifikation, Kundendoku, 8 Schulungsvideos, Quellcode, technische Doku

**Nicht im Auftrag (Annahme: nicht v1):** Login/Benutzerkonten, Fortschrittsstatistik, Mehrbenutzer-Netzwerkbetrieb (der Host lauscht nur lokal), öffentliche Web-Version.

**Auftragslücken (brauchen Entscheidung):** Rollen/Zugriffsschutz, Prüfmechanismus, Isolation, Zielplattform.

## 3. Optionen und Empfehlungen

### 3.1 SQL-Engine / Dialekt (kritisch, bestimmt das Importformat)

| Option | Pro | Contra |
|---|---|---|
| **A. SQLite eingebettet (Empfehlung)** | Keine Installation, eine Datei pro Übungs-DB, schnell, einfach zu isolieren und zu sichern | Dialekt-Eigenheiten (kein RIGHT/FULL JOIN vor 3.39, schwache Typisierung, kein Schema-Konzept); Azubi-Prüfung läuft oft gegen SQL Server/MySQL |
| B. SQL Server LocalDB / Express | Nahe an Prüfungsrealität (T-SQL) | Installation und Rechte, größere Abhängigkeit, schwer zu isolieren, Risiko für „fehlerfreie Installation“ |
| C. PostgreSQL/MySQL mitgeliefert | Standardnaher Dialekt | Serverprozess, Installer-Aufwand, Betrieb auf Schul-PCs |

**Entschieden (Mensch): A, SQLite.** Folge: `datenbank.sql` der Kunden muss SQLite-Dialekt sein. Das ist dem Kunden **vorab** mitzuteilen und in der Spezifikation festzuschreiben.

### 3.2 Tech-Stack und Hülle

**Entschieden (Mensch, 2026-10-22):**
- UI mit **Blazor (Interactive Server)** und **MudBlazor** als Komponentenbibliothek (kam mit dem Template `SqlApp`)
- Hülle = **lokaler ASP.NET-Core-Webserver + Browser**
- Begründung: Mitentwickler arbeiten auch unter Linux. WPF/BlazorWebView läuft dort nicht, MAUI ebenfalls nicht.
- Verworfen: Avalonia, WPF + BlazorWebView (am 2026-10-01 entschieden, am 2026-10-22 ersetzt), MAUI Blazor Hybrid

| Option | Pro | Contra | Stand |
|---|---|---|---|
| A. MAUI Blazor Hybrid | Native Hülle | Kein Linux, schweres Tooling | verworfen |
| B. WPF + BlazorWebView | Einfach, ausgereift | Nur Windows | verworfen |
| **C. Blazor Server lokal + Browser** | Läuft überall, wo .NET läuft; Template liegt vor; kein WebView-Setup | Lokaler Port, wirkt weniger wie Desktop-App, Zugriffsschutz nötig (3.6), Firewall-Rückfragen möglich | **gewählt** |
| D. Photino.Blazor (native Fenster-Hülle) | Fenster statt Browser-Tab, plattformübergreifend | Zusätzliche Abhängigkeit; Linux-Details nicht geprüft | Option später als zweite Hülle |

Die Razor-Komponenten liegen in einer eigenen Razor Class Library, damit eine zweite Hülle (D) später möglich bleibt.
SQL-Editor: fertige Web-Komponente (z. B. Monaco oder CodeMirror per JS-Interop) mit SQL-Highlighting, kein Eigenbau.

**Auflagen für den Host (`SqlTrainer.App`):**
- Freier Port statt fest 5166 (Port 0 bzw. freien Port ermitteln), die tatsächliche URL beim Start öffnen
- Nur localhost binden (Loopback), nicht an alle Adapter
- Irreführende Ausgabe „Accessible at <LAN-IP>“ entfernen; der Server lauscht gar nicht im LAN
- Konsolenfenster unter Windows ausblenden (WinExe) oder klares Beenden anbieten; Schließen des Browser-Tabs beendet die App nicht, das muss für Anwender erkennbar gelöst sein (z. B. „Beenden“-Eintrag in der App)
- *Ergänzung K1, zur Bestätigung:* Host-Header einschränken (`AllowedHosts`/Prüfung auf localhost), damit andere Webseiten im Browser den lokalen Server nicht per DNS-Rebinding ansprechen können
- *Ergänzung K1, zur Bestätigung:* Nur eine Instanz pro Benutzer; ein zweiter Start öffnet die laufende Instanz, statt eine zweite auf dieselbe `app.db` zu starten

### 3.2a UI-Architektur

**Entschieden (Mensch): leichtes MVVM** (Begründung: der Entwickler kennt MVVM). Verworfen: MVC (in Interactive Server läuft die Interaktion über eine SignalR-Verbindung, nicht über Controller-Requests; gegenüber MVVM kein Vorteil), MVP (mehr Boilerplate in Blazor bei gleichem Testnutzen), MVU/Flux (für ca. 6 Screens überdimensioniert).

Regeln:
- **View:** Razor-Komponente, nur Markup und Bindings, keine Fachlogik
- **ViewModel:** normale C#-Klasse in `SqlTrainer.UI`, hält den Screen-Zustand, ruft Services auf, meldet Änderungen über ein Ereignis (`OnChange`); die Komponente ruft darauf `StateHasChanged`. Kein `ICommand`, kein MVVM-Framework
- **Lebensdauer: Scoped registrieren, nicht Singleton.** Bei Interactive Server gibt es einen Circuit pro Browser-Tab; Scoped heißt ein ViewModel pro Tab. Bei Singleton würden sich zwei Tabs den Zustand teilen
- **Tab neu laden = neuer Circuit = ViewModel-Zustand weg.** Was nach einem Reload erhalten bleiben soll (gewählte DB, Thema, Aufgabe), steht in der URL (Routing), nicht im ViewModel
- **Model/Services:** `Core`, `Import`, `Execution`, `Storage` hinter Interfaces, ohne Abhängigkeit zur UI
- Mehrere Tabs bedeuten parallele Schreibzugriffe auf `app.db`: Zugriffe über `Storage` bündeln, SQLite mit WAL und `busy_timeout` betreiben, Schreibvorgänge kurz halten
- Einfache Screens (Datenbankliste, Bestätigungsdialoge) brauchen kein ViewModel; Richtschnur, keine Dogmatik
- ViewModels bekommen Unit-Tests ohne UI/bUnit

Geplante ViewModels: Aufgabenscreen (Editor, Ausführen, Prüfen, Hinweis, Musterlösung), Import (ZIP wählen, Fehlerbericht), Aufgabenbearbeitung (Verwaltung).

Abhängigkeiten zeigen nur nach innen: `App` → `UI` → `Application` → `Core` ← `Import`/`Execution`/`Storage`. `Import`, `Execution` und `Storage` kennen sich nicht gegenseitig; die Use Cases in `Application` verbinden sie. ViewModels rufen nur `Application`-Services auf. Ein ArchUnitNET-Test erzwingt diese Richtung.

### 3.3 Struktur (grob, unabhängig vom UI-Framework)

Modulare Schichten, damit die Hülle austauschbar bleibt:

```
src/
  SqlTrainer.Core        Domänenmodell (Übungs-DB, Thema, Aufgabe), Interfaces
  SqlTrainer.Import      ZIP lesen, Format validieren, in Speicher überführen (kein UI)
  SqlTrainer.Execution   SQL ausführen, Ergebnis vergleichen (Prüfung)
  SqlTrainer.Storage     App-Metadaten (Aufgaben, DB-Liste), Dateiablage
  SqlTrainer.Application Use Cases: Import-Ablauf, Lösung prüfen, DB löschen, Aufgabe bearbeiten
  SqlTrainer.UI          Razor Class Library: Blazor-Komponenten, Seiten, ViewModels
  SqlTrainer.App         ASP.NET-Core-Host (Kestrel, nur localhost), Composition Root (DI)
tests/
  SqlTrainer.<Modul>.Tests         je Modul ein Testprojekt
  SqlTrainer.Architecture.Test     ArchUnitNET: Abhängigkeitsrichtung
docs/
```

Gemeinsame Build-Einstellungen: `Directory.Build.props`, `.editorconfig`, `BannedSymbols.txt`.
`Core` enthält nur Modelle und Interfaces, darunter der Typ `ResultSet`, den `Execution` zurückgibt. Er wird früh festgelegt, damit die Prüfregeln später austauschbar bleiben.

Speicherung:
- `app.db` (SQLite): Übungs-DBs (Name, Beschreibung, Pfade), Aufgaben (Nr, Thema, Text, Hilfe, Musterlösung)
- Pro Übungs-DB eine eigene SQLite-Datei (aus `datenbank.sql` erzeugt) plus ER-PNG im Datenordner
- Datenordner pro Benutzer über `Environment.SpecialFolder.LocalApplicationData` (Windows: `%LOCALAPPDATA%`, Linux: `~/.local/share`), damit keine Adminrechte nötig sind. Pfade nie mit `\` zusammensetzen, immer `Path.Combine`
- *Ergänzung K1:* Dateien im Datenordner heißen nach einer erzeugten ID, nicht nach dem Namen aus dem ZIP. Das vermeidet Groß-/Kleinschreibungs-Kollisionen unter Linux, ungültige Zeichen und Pfadtricks
- Datensicherung = Datenordner kopieren (muss in der Kundendoku für die gelieferten Betriebssysteme stehen)

### 3.4 Ausführung und Isolation

Empfehlung: Eingaben der Lernenden laufen **in-process gegen die Übungs-DB, schreibgeschützt**.
- Verbindung read-only öffnen (`Mode=ReadOnly` bzw. `PRAGMA query_only=ON`), so dass auch DDL/DML fehlschlägt
- Genau ein Statement pro Eingabe, nur SELECT erlaubt (Fehlermeldung statt Ausführung bei anderem). Ein einzelnes abschließendes `;` ist erlaubt (das Musterlösungs-Beispiel des Kunden endet mit `;`), ein zweites Statement nicht
- Timeout (z. B. 5 s, per Interrupt/Cancellation) und Zeilenlimit (z. B. 1000) gegen Endlosabfragen und Kreuzprodukte
- Keine Dateisystem-/Extension-Funktionen (`load_extension` aus, `ATTACH` unterbunden)

Alternative: Schreibende Statements erlauben, aber auf einer Wegwerf-Kopie. Für die 5 Themen (alles SELECT) nicht nötig; nicht v1.

### 3.5 Prüfmechanismus

Empfehlung: **Ergebnisvergleich** statt SQL-Textvergleich. Die Musterlösung wird gegen dieselbe Übungs-DB ausgeführt, das Ergebnis mit dem der Nutzereingabe verglichen.

Vorschlag Vergleichsregeln (Entscheidung nötig):
- Spaltenanzahl muss gleich sein; Spaltenüberschriften/Aliase werden **nicht** verglichen, Werte nach Position
- Zeilenreihenfolge nur relevant, wenn die Musterlösung ein `ORDER BY` enthält; sonst Mengenvergleich (Multimenge, Duplikate zählen)
- Zahlenvergleich mit kleiner Toleranz bei Fließkomma; `NULL` gleich `NULL`
- Rückmeldung nur richtig/falsch plus kurzer Grund (z. B. „Spaltenanzahl abweichend“, „Zeilenanzahl abweichend“), keine Lösungsverrat-Details

Alternativen: reiner Textvergleich (zu streng, abgelehnt); Abgleich per Parser/AST (aufwendig, abgelehnt).
Risiko: Ergebnisgleichheit ist bei kleinen Beispieldaten manchmal zufällig erfüllt. Gegenmaßnahme in der Importvalidierung/Doku: Beispieldaten so wählen, dass naheliegende Fehlversuche abweichen.

### 3.6 Rollen / Zugriffsschutz der Verwaltung (Entscheidung B)

Auftrag nennt Lehrkräfte/Admins, aber keinen Login. Mit dem lokalen Webserver wird das gewichtiger: Der Server ist von jedem Benutzer und Prozess auf dem Rechner erreichbar. Auf geteilten Schul-PCs könnte ein Lernender die Verwaltung öffnen und Datenbanken löschen.

| Option | Beschreibung |
|---|---|
| A. Verwaltung ohne Schutz | Einfach; jeder mit Zugriff auf die App kann löschen und importieren |
| **B. Verwaltung mit PIN (Empfehlung)** | PIN gehasht lokal gespeichert, Festlegung beim ersten Start, Zurücksetzen dokumentiert; geringer Aufwand |
| C. Echte Benutzerkonten | Nicht beauftragt; deutlich mehr Aufwand |

Empfehlung B, **PIN als Pflicht** statt wie bisher „optional“. Das Löschen bleibt zusätzlich bestätigungspflichtig (Auftrag). Die PIN schützt vor Lernenden an derselben App; vor anderen Prozessen/Webseiten schützen die Host-Auflagen in 3.2 (localhost, Host-Header).
Offene Teilfrage: Wie wird eine vergessene PIN zurückgesetzt (Datei im Datenordner löschen, in der Kundendoku beschrieben)?

## 4. Importformat

Eigenes Dokument: `Draft Importformat.md` (Spezifikation im Entwurf, mit offenen Entscheidungen).

## 5. Etappen

Ausführlich im freigegebenen Etappenplan (K1-Session, 2026-10-29); hier die Kurzfassung.

**Ausgangslage im Repo:** Das Template `SqlApp` liegt im Root (net10.0, MudBlazor 9.*, eingebettete Assets, Single-File-Publish; `Program.cs` hat noch Festport 5166, `UseHttpsRedirection` und die LAN-IP-Ausgabe). Auf `feature/solution-skeleton` (Commit `53bc499`) liegen Core/Execution/UI-Projekte und eine nun obsolete WPF-App.

0. **Entscheidungen und Aufräumen (K1, Management, Mensch):** K1-Docs committen; Importformat-Lücken 1–6 entscheiden, Draft final machen; Fragen an den Kunden (A, B, „Desktop-App“)
1. **Solution-Skelett (D1, Branch `feature/init`):** alle `src/`- und `tests/`-Projekte, Template nach `src/SqlTrainer.App` verschieben (WPF-App entfällt, Core/Execution/UI aus `53bc499` übernehmen), Host-Auflagen aus 3.2, `Core`-Modelle/Interfaces und `ResultSet`, DI mit Scoped-ViewModels, Build-Einstellungen, ArchUnitNET-Test, CI (Build + Test unter Windows und Linux). `feature/init` ist im geteilten Checkout ausgecheckt; D übernimmt ihn erst, wenn die uncommitteten Dateien committet oder verschoben sind. `feature/solution-skeleton` wird danach von Management entfernt. Spätere Etappen: eigener Branch/Worktree
2. **Execution + Storage (parallel zu 0):** Read-only-Ausführung, Timeout, Zeilenlimit, Prüf-Vergleich hinter Interface; `app.db`, Datei pro Übungs-DB, WAL, parallele Schreiber testen
3. **Import (sobald Format final):** ZIP-Stream, Validierung, Trockenlauf in Temp-DB, atomares Speichern; je Regel ein Testpaket
4. Use Cases in `Application` und Lernenden-Ablauf in der UI (DB → Thema → Aufgabe → Editor → Prüfen/Hinweis/Musterlösung)
5. Verwaltung: Liste, Import, Löschen mit Bestätigung, Aufgaben bearbeiten, PIN je nach Entscheidung B
6. Inhalte: 5 Übungs-DBs × 25 Aufgaben (ab finalem Format, jede DB durch den Importer prüfen)
7. Auslieferung (Single-File-Export je Zielsystem nach Entscheidung A), Kundendoku, Videos, technische Doku

## 6. Risiken

1. SQL-Dialekt-Mismatch zwischen Kundenskripten und Engine (Mitigation: Dialekt früh festlegen, Importvalidierung führt Skript und Musterlösungen testweise aus)
2. Zu strenge/zu lockere Ergebnisprüfung (Mitigation: dokumentierte Regeln, Testsuite mit Positiv-/Negativfällen)
3. Zielplattform für den Kunden offen (Entscheidung A)
4. Zeitplan: Inhalte (125 Aufgaben) sind ein eigener, nicht kleiner Aufwand
5. Lokaler Webserver ist für andere lokale Benutzer, Prozesse und Webseiten erreichbar (Mitigation: nur localhost, Host-Header-Prüfung, PIN für die Verwaltung)
6. Auftrag verlangt eine „Desktop-App“; ein lokaler Host im Browser könnte bei der Abnahme als Abweichung gelten (Mitigation: Browser öffnet automatisch, kein sichtbares Konsolenfenster, Startverknüpfung, „Beenden“ in der App; Photino.Blazor als zweite Hülle; Frage an den Kunden in Entscheidung A mitnehmen)
7. Unterschiede Windows/Linux (Pfade, Groß-/Kleinschreibung, Zeilenenden) (Mitigation: CI-Tests auf beiden Systemen, Vorgaben in 3.3 und im Importformat)
8. Mehrere Tabs/Fenster: parallele Schreibzugriffe auf `app.db` (Mitigation: 3.2a)

## 7. Offene Entscheidungen (Übersicht)

| # | Entscheidung | Empfehlung |
|---|---|---|
| 1 | ~~SQL-Dialekt / Engine~~ **entschieden:** SQLite eingebettet | – |
| 2 | ~~UI-Technologie und Hülle~~ **entschieden:** Blazor Server + MudBlazor, lokaler ASP.NET-Core-Host + Browser (WPF verworfen, 2026-10-22) | – |
| 3 | Prüfregeln (Abschnitt 3.5) | Ergebnisvergleich, Reihenfolge nur bei ORDER BY |
| A | **Zielplattform für den Kunden (neu):** nur Windows-Build oder Windows + Linux? | Windows-Build als Auslieferung. Begründung: Zielsysteme eines Ausbildungsinstituts sind sehr wahrscheinlich Windows (Annahme, beim Kunden erfragen); zwei Builds verdoppeln Kundendoku, Abnahme und die 8 Schulungsvideos. Die Entwicklung bleibt Linux-fähig, Tests laufen in der CI auf beiden Systemen, Windows-Test vor jedem Release. Linux nachziehen, falls der Kunde es verlangt |
| B | **Zugriffsschutz Verwaltung (früher #4, verschärft):** siehe 3.6 | PIN als Pflicht, plus Host-Auflagen aus 3.2 |
| 5 | Importformat-Lücken 1–6 offen (Lücke 7 durch SQLite geschlossen) | siehe `Draft Importformat.md` |
| 6 | Aufgaben in der App bearbeiten: Grenzen (nur Texte oder auch Themenzuordnung/Anzahl?) | Texte und Musterlösung editierbar, Neuanlage erlaubt, Importgrenze 25 gilt nur für Pakete |
