# STATUS – Konzept Architektur (K1)

Stand: 2026-10-22

## Ausgangslage
- Anforderungen liegen vor: Kundenauftrag (`docs/Kundenauftrag Datenbanktrainer.pdf`, Kerninhalt in `plan.md` Abschnitt 2) und Importformat-Vorgabe des Kunden (in `Draft Importformat.md` Abschnitt 1).
- Template `SqlApp` (Blazor Web App mit MudBlazor, Single-File-Export) liegt laut Management auf `main` (PR #1). K1 hat es nicht selbst gelesen.
- Diese Dateien sind untracked im geteilten Checkout (`feature/init`); K1 committet nichts.

## Getroffene Entscheidungen (vom Menschen)
- Repo-Struktur `docs/`, `src/`, `tests/`; Solution `sql-trainer-app.slnx`
- Branch-Regeln: `main` geschützt, `feature/*` und `bugfix/*`, nur PR + 1 Review, Squash
- SQL-Engine/Dialekt: SQLite eingebettet (Importformat-Lücke 7 damit geschlossen)
- UI-Muster: leichtes MVVM (ViewModels als **Scoped** DI-Klassen mit OnChange-Ereignis, nur für Screens mit Zustand; MVC/MVP/Flux verworfen)
- UI-Technologie und Hülle (2026-10-22): Blazor Server (Interactive Server) mit MudBlazor, Hülle = lokaler ASP.NET-Core-Webserver + Browser. **WPF + BlazorWebView ist verworfen** (Linux-Entwickler; WPF und MAUI laufen dort nicht). Photino.Blazor bleibt Option für später als zweite Hülle.
- Schichten bleiben: Core / Import / Execution / Storage, UI als Razor Class Library, `SqlTrainer.App` ist jetzt ASP.NET-Core-Host.

## Dokumente
- `plan.md`: Scope, Entscheidungen, Optionen mit Empfehlung (Engine, Stack, Hülle, Struktur, Ausführung, Prüfung, Zugriffsschutz), Arbeitspakete, Risiken
- `Draft Importformat.md`: Kundenvorgabe, Mapping der Themen-Keys, Empfehlungen zu den Formatlücken 1–6 (7 geschlossen), Validierungsregeln, Linux/Windows-Regeln

## Offene Entscheidungen (alle mit Empfehlung, noch nicht entschieden)
- **A. Zielplattform für den Kunden** → Windows-Build als Auslieferung; Entwicklung Linux-fähig, CI auf beiden Systemen. Beim Kunden erfragen, welche Betriebssysteme er einsetzt, und ob ein lokal gestarteter Host im Browser als „Desktop-App“ gilt.
- **B. Zugriffsschutz Verwaltung** (früher #4, verschärft) → PIN als Pflicht, plus Host-Auflagen (nur localhost, Host-Header-Prüfung)
- 3. Prüfregeln → Ergebnisvergleich, Zeilenreihenfolge nur bei `ORDER BY` in der Musterlösung, Aliase egal
- 5. Importformat-Lücken **1–6** → siehe `Draft Importformat.md` Abschnitt 2 (u. a. mehrzeilige Felder, ZIP mit Unterordner, nur UTF-8, Namenskollision = Ablehnung, Gesamtpaket bei Fehler ablehnen; neu: Groß-/Kleinschreibung unter Linux, CRLF/LF, `\` in ZIP-Pfaden)
- 6. Grenzen der Aufgabenbearbeitung in der App

## Entschieden am 2026-10-29 (Mensch, im K1-Chat)
- Eigene **Application-Schicht**; Projektnamen bleiben `SqlTrainer.Core/Import/Execution/Storage/Application/UI/App`
- Template `SqlApp` wird nach `src/SqlTrainer.App` verschoben; die WPF-App entfällt
- Solution-Skelett wird auf **`feature/init`** gebaut, nicht auf `feature/solution-skeleton`
- Etappenplan 0–7 freigegeben (siehe `plan.md` Abschnitt 5). Darin enthalten und damit freigegeben: ArchUnitNET-Architekturtest, `Directory.Build.props`, `.editorconfig`, `BannedSymbols.txt`, CI-Build auf Windows und Linux

## Noch zu bestätigen
- Die Host-Auflagen mit Kennzeichen „Ergänzung K1“ in `plan.md` 3.2 und 3.3 (Host-Header, Einzelinstanz, ID-Dateinamen) sind meine Zusätze. Sie stehen im Etappenplan, wurden aber nicht einzeln bestätigt.
- B (PIN) wurde vom Menschen im Chat in Richtung „optional, erst nach Rückfrage beim Kunden“ diskutiert, aber nicht entschieden; `plan.md` 3.6 empfiehlt noch „Pflicht“.

## Anmerkungen
- Die Themen-Keys `AGGREGAT`/`GRUPPIERUNG` weichen von den Auftragsbezeichnungen ab; Mapping ist dokumentiert.
- Entfallen mit dem Wechsel der Hülle: WebView2-Prüfung im Installer und die Mitteilung „nur Windows“ an den Kunden.
- Das Format muss laut Auftrag **vor** der Implementierung final dokumentiert sein → Entscheidungen 5 und A sind der kritische Pfad.

## Nächster Schritt
1. Management: K1-Docs committen (Voraussetzung dafür, dass D den Checkout auf `feature/init` übernehmen kann) und D1 für Etappe 1 starten.
2. Mensch: Importformat-Lücken 1–6 entscheiden (kritischer Pfad für Etappe 3), außerdem A, B, Prüfregeln 3 und 6. A und die Desktop-App-Frage gehen als Rückfrage an den Kunden.
3. K1: danach den Draft in die finale Spezifikation überführen und `plan.md` auf „entschieden“ setzen.
