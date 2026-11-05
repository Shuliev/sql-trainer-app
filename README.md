# sql-trainer-app

Eine Blazor-Server-Web-App (Render-Modus Interactive Server) mit MudBlazor.

## Kurzbeschreibung

#### App starten (aus `src/SqlTrainer.App/`):
```bash
dotnet watch
```

#### App veröffentlichen (aus `src/SqlTrainer.App/`) nach `./publish/win-x64/SqlTrainer.App.exe`:
```bash
dotnet publish -c Release -r win-x64 -o ./publish/win-x64
```

## Ausführliche Beschreibung

## Architektur

Die App läuft lokal: Ein ASP.NET-Core-Host liefert eine Blazor-Server-Oberfläche (MudBlazor)
an den Browser auf `127.0.0.1` aus. Die Übungsdatenbanken sind SQLite-Dateien. Es gibt keinen
Cloud-Anteil.

Die Solution ist in Schichten aufgeteilt. Abhängigkeiten zeigen nur nach innen, in Richtung
`SqlTrainer.Core`:

```
SqlTrainer.App  (Host, Composition Root)
  ├─► SqlTrainer.UI ──► SqlTrainer.Application ──► SqlTrainer.Core
  └─► SqlTrainer.Import, SqlTrainer.Execution, SqlTrainer.Storage ──► SqlTrainer.Core
```

### Projekte in `src/`

| Projekt | Zweck | Hängt ab von |
|---|---|---|
| `SqlTrainer.Core` | Domänenmodelle (`ExerciseDatabase`, `Exercise`, `Topic`, `ResultSet`, `CheckResult`, …) und die Interfaces, die die anderen Schichten implementieren (`IQueryExecutor`, `IResultComparer`, `IExerciseRepository`, `IExerciseDatabaseRepository`). Keine Logik, die eine Datenbank, Dateien oder eine Oberfläche braucht. | nichts |
| `SqlTrainer.Application` | Use Cases, die die Module verbinden, z. B. ein Paket importieren, die Abfrage eines Lernenden prüfen, eine Datenbank löschen, eine Aufgabe bearbeiten. Die ViewModels rufen diese Services auf. *(noch leer)* | Core |
| `SqlTrainer.Import` | Liest ein Importpaket (ZIP mit `beschreibung.txt`, `er_diagramm.png`, `datenbank.sql`, `aufgaben.txt`) und prüft es. Format: [`docs/Konzept Architektur/Draft Importformat.md`](docs/Konzept%20Architektur/Draft%20Importformat.md). *(noch leer)* | Core |
| `SqlTrainer.Execution` | Führt SQL gegen eine Übungsdatenbank in SQLite aus: schreibgeschützt, genau ein `SELECT`, mit Timeout und Zeilenlimit. Vergleicht das Ergebnis des Lernenden mit dem Ergebnis der Musterlösung. *(noch leer)* | Core |
| `SqlTrainer.Storage` | Speichert Daten auf der Festplatte: `app.db` (Datenbanken und Aufgaben), pro Übungsdatenbank eine SQLite-Datei plus ER-Diagramm, im Datenordner des Benutzers. *(noch leer)* | Core |
| `SqlTrainer.UI` | Razor Class Library: Seiten, Layout und ViewModels (leichtes MVVM, ViewModels als Scoped registriert, eines pro Browser-Tab). | Application |
| `SqlTrainer.App` | ASP.NET-Core-Host: startet den lokalen Webserver, öffnet den Browser, verdrahtet die Dependency Injection (Composition Root). Erzeugt die Single-File-`.exe`. | alle Projekte oben |

### Projekte in `tests/`

Jedes Projekt in `src/` außer `SqlTrainer.App` hat ein Testprojekt `tests/SqlTrainer.<Projekt>.Tests`.
`SqlTrainer.Architecture.Tests` prüft mit ArchUnitNET die Abhängigkeitsregeln: Core hängt von
keiner anderen Schicht ab; Import, Execution, Storage und Application hängen nur von Core ab; UI
hängt weder vom Host noch von Import/Execution/Storage ab. Eine verbotene Referenz lässt diese
Tests fehlschlagen, lokal und in der CI.

Mehr Hintergrund und offene Entscheidungen: [`docs/Konzept Architektur/plan.md`](docs/Konzept%20Architektur/plan.md).

## Einstieg (Entwicklung)

### Voraussetzungen
- [.NET 10 SDK](https://dotnet.microsoft.com/download) auf dem Entwicklungsrechner installiert.

### .NET 10 unter Windows prüfen oder installieren

1. Wenn du nicht sicher bist, ob die richtige SDK-Version installiert ist, öffne die
   **Eingabeaufforderung** oder **PowerShell** und führe aus:

   ```powershell
   dotnet --list-sdks
   ```

2. Enthält die Ausgabe eine Version, die mit `10.0.` beginnt, ist das .NET 10 SDK installiert.
   Wird `dotnet` nicht erkannt oder gibt es keine Version `10.0.x`, installiere das SDK.
3. Lade das **.NET 10 SDK** für Windows von der
   [offiziellen .NET-10-Downloadseite](https://dotnet.microsoft.com/download/dotnet/10.0) herunter.
   Wähle den Installer für deine Windows-Architektur (meist x64), führe ihn aus und
   öffne danach ein neues Eingabeaufforderungs- oder PowerShell-Fenster.
4. Führe `dotnet --list-sdks` erneut aus und prüfe, dass ein SDK `10.0.x` erscheint.

### Lokal ausführen (Entwicklung)

Ohne Hot Reload starten:
```bash
dotnet run
```

Mit Hot Reload starten (Browser lädt bei Codeänderungen automatisch neu):
```bash
dotnet watch
```

Die App lauscht nur auf `http://127.0.0.1` (von anderen Geräten aus nicht erreichbar) und auf
einem freien Port, der beim Start gewählt wird. Die Konsole zeigt die tatsächliche URL, und der
Standardbrowser öffnet sie automatisch.

Optionen (Kommandozeile oder `appsettings*.json`):
- `--Port 5166` verwendet einen festen Port statt eines freien.
- `--OpenBrowser false` öffnet den Browser nicht (z. B. für automatisierte Läufe).

Beispiel: `dotnet run -- --Port 5166 --OpenBrowser false`

## Veröffentlichen als SqlTrainer.App.exe

Die App wird als **eine einzige, vollständig eigenständige `SqlTrainer.App.exe`** für Windows
veröffentlicht.

Im Projektordner (`src/SqlTrainer.App/`) ausführen:

```bash
dotnet publish -c Release -r win-x64 -o ./publish/win-x64
```

Ergebnis:

```
publish/win-x64/
  SqlTrainer.App.exe   <- das ist die komplette App. Einfach starten.
  appsettings.json     <- optional (Logging-Konfiguration); kann gelöscht werden, wenn nicht benötigt
```

Damit die `.exe` ohne weitere Installation läuft, sind die .NET-Runtime und alle statischen
Web-Assets (CSS, JS, Favicon) in die `.exe` eingebettet.

> Hinweis: `SqlTrainer.App.exe` ist etwa 50 MB groß, weil sie die komplette .NET-Runtime und
> alle statischen Web-Assets enthält. Das ist der erwartete Preis für eine echte
> Single-File-Anwendung ohne Abhängigkeiten.

### Was beim Start von `SqlTrainer.App.exe` passiert

- Sie startet einen lokalen Webserver auf `http://127.0.0.1` mit einem freien Port.
- Sie öffnet automatisch den Standardbrowser mit dieser URL.
- „Beenden“ (Power-Symbol oben rechts in der App), das Schließen des Konsolenfensters oder
  `Strg+C` beendet den Server. Das Schließen des Browser-Tabs beendet ihn nicht.

### Veröffentlichen für andere Plattformen

Ersetze `win-x64` durch einen anderen [Runtime Identifier (RID)](https://learn.microsoft.com/dotnet/core/rid-catalog),
um ein anderes Betriebssystem bzw. eine andere Architektur zu erzeugen, z. B.:

```bash
dotnet publish -c Release -r linux-x64 -o ./publish/linux-x64
dotnet publish -c Release -r osx-arm64 -o ./publish/osx-arm64
```

(Der automatische Browserstart und das Single-File-Paket sollten auf diesen Plattformen genauso
funktionieren.)
