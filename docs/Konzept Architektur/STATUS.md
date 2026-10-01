# STATUS – Konzept Architektur (K1)

Stand: 2026-10-01

## Ausgangslage
- Repo enthält nur `README.md` (eine Zeile), `CLAUDE.md` und eine .NET-typische `.gitignore`. Kein Code, keine Anforderungen.
- Die .gitignore deutet auf einen .NET-Stack hin. Das ist **keine** Entscheidung, nur ein Indiz.

## Getroffene Entscheidungen
Keine. Der Mensch liefert Anforderungen und Kontext nachträglich („ich gebe dir alles später“).

## Offene Punkte (brauchen Input vom Menschen)
1. Zielgruppe und Einsatzkontext (Selbstlernen, Ausbildung/Kurs, Prüfungsvorbereitung, privates Lernprojekt)
2. Scope v1: Muss-Features, bewusst ausgeschlossene Features
3. Tech-Stack-Vorgaben oder -Präferenzen (Sprache, Framework, Hosting)
4. Ziel-SQL-Dialekt (SQLite, PostgreSQL, SQL Server, …)
5. Ausführung der Übungen: serverseitig oder im Browser, Isolation und Limits
6. Validierung: Ergebnisvergleich gegen Musterlösung oder andere Kriterien
7. Nutzerverwaltung, Persistenz von Fortschritt, Betrieb und Deployment

## Nächster Schritt
Sobald der Mensch die Anforderungen liefert: Fragen 1–7 durchgehen, Ansätze mit Trade-offs vorschlagen, Entscheidungen in `plan.md` festhalten. `plan.md` wird erst dann angelegt.
