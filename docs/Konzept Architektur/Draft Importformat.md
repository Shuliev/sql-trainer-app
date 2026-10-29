# Draft – Spezifikation Importformat „Trainingsdatenbank“

Stand: 2026-10-22 · Status: **Entwurf**. Offen sind die Lücken 1–6; Lücke 7 (SQL-Dialekt) ist durch die SQLite-Entscheidung geschlossen. Abschnitt 1 ist Kundenvorgabe (wörtlich bzw. sinngemäß). Alles mit „Empfehlung“ ist **noch nicht entschieden**.

## 1. Kundenvorgabe

Eine komplette Trainingsdatenbank wird als ZIP hochgeladen:

```
Name_der_Datenbank.zip
├── beschreibung.txt   Beschreibung der DB und des Szenarios
├── er_diagramm.png    ER-Diagramm als PNG
├── datenbank.sql      SQL-Skript: Tabellen erstellen + Beispieldaten einfügen
└── aufgaben.txt       25 Aufgaben inkl. Hilfestellungen und Musterlösungen
```

`aufgaben.txt`: UTF-8, Feldtrenner `###TRENN###`, fünf Felder:

```
Nr###TRENN###Themenbereich###TRENN###Aufgabe###TRENN###Hilfestellung###TRENN###Musterloesung
```

Beispiel:

```
1###TRENN###SELECT###TRENN###Gib alle Kunden aus.###TRENN###Verwende SELECT *.###TRENN###SELECT * FROM kunde;
```

25 Aufgaben, je fünf aus: `SELECT`, `AGGREGAT`, `JOIN`, `GRUPPIERUNG`, `SUBSELECT`.

Das Importpaket enthält damit auch die Aufgaben (löst die Unklarheit im Kundenauftrag-PDF).

### Mapping Themen-Keys → Anzeigenamen

| Key in `aufgaben.txt` | Anzeige in der App (Bezeichnung im Auftrag) |
|---|---|
| `SELECT` | SELECT |
| `AGGREGAT` | Aggregatfunktionen |
| `JOIN` | JOIN |
| `GRUPPIERUNG` | GROUP BY |
| `SUBSELECT` | Subselects |

## 2. Zu klärende Lücken (Entscheidung nötig)

| # | Frage | Empfehlung |
|---|---|---|
| 1 | Datensatz pro Zeile? Mehrzeilige Musterlösungen/Texte? | Mehrzeilige Felder **erlaubt**. Ein neuer Datensatz beginnt an einer Zeile, die mit `<Ziffern>###TRENN###` anfängt. Zeilenumbruch am Feldende wird abgeschnitten. Dann sind Musterlösungen mit Umbrüchen (lesbare SQL) möglich |
| 2 | Kopfzeile? | Keine Pflicht. Erste Zeile `Nr###TRENN###Themenbereich…` wird, falls vorhanden, ignoriert |
| 3 | ZIP-Struktur, Dateinamen | Dateien im Root **oder** in genau einem Unterordner (Windows „Komprimierter Ordner“). Dateinamen unabhängig von Groß-/Kleinschreibung: Das braucht echten Code (Vergleich mit `OrdinalIgnoreCase` auf den ZIP-Einträgen), weil unter Linux Dateinamen case-sensitiv sind; es darf nicht am Dateisystem hängen. Gibt es zwei Einträge, die sich nur in der Schreibweise unterscheiden: Ablehnung. Pfadtrenner in Eintragsnamen normalisieren: ZIPs aus Windows-Werkzeugen enthalten teils `\` statt `/`. Zusatzdateien werden ignoriert (Warnung). Fehlt eine der vier Pflichtdateien: Ablehnung. Pfade mit `..` oder absolute Pfade (nach der Normalisierung geprüft): Ablehnung (Zip-Slip). Größenlimit z. B. 50 MB entpackt. Gelesen wird direkt aus dem ZIP-Stream; das Entpacken auf die Platte entfällt, damit Dateinamen aus dem ZIP nie zu Pfaden werden |
| 4 | Encoding von `beschreibung.txt`, `datenbank.sql` | Alle Textdateien UTF-8 (BOM toleriert). Anderes Encoding: Ablehnung mit klarer Meldung, kein Raten. Zeilenenden CRLF und LF werden in allen Textdateien akzeptiert (auch gemischt); intern wird auf LF normalisiert, bevor Datensätze erkannt werden |
| 5 | DB-Name, Kollision | Name = ZIP-Dateiname ohne `.zip`; `_` wird in der Anzeige zu Leerzeichen. Existiert der Name bereits: Ablehnung mit Hinweis, erst löschen. Aufgaben lassen sich in der App bearbeiten, daher ist „Ersetzen“ in v1 nicht nötig |
| 6 | Validierung / Fehlerverhalten | Siehe Abschnitt 3. Bei Fehlern wird das **gesamte Paket abgelehnt**, nichts wird gespeichert; Fehlerliste vollständig (nicht nur der erste Fehler) |
| 7 | SQL-Dialekt von `datenbank.sql` | **Entschieden: SQLite** (`plan.md` 3.1). Dem Kunden vor Beginn der Inhaltserstellung mitteilen |

## 3. Validierungsregeln (Empfehlung)

Reihenfolge, alles wird gesammelt gemeldet:

1. **ZIP:** lesbar, Pflichtdateien vorhanden, keine unzulässigen Pfade, Größenlimit
2. **`er_diagramm.png`:** PNG-Signatur gültig (nicht nur die Endung)
3. **`beschreibung.txt`:** UTF-8, nicht leer
4. **`aufgaben.txt`:**
   - UTF-8, jeder Datensatz hat genau 5 Felder
   - `Nr` ist ganzzahlig und eindeutig; Empfehlung: genau 1–25
   - Themen-Key ist einer der 5 gültigen (unabhängig von Groß-/Kleinschreibung)
   - genau 25 Aufgaben, genau 5 pro Thema
   - Aufgabe und Musterlösung nicht leer; Hilfestellung nicht leer (Auftrag verlangt Hilfe je Aufgabe)
5. **`datenbank.sql`:** wird in einer **temporären** Datenbank ausgeführt; Fehler bei Ausführung = Ablehnung (mit Zeilenangabe, soweit möglich)
6. **Musterlösungen:** jede wird gegen die temporäre DB ausgeführt; Syntaxfehler/Laufzeitfehler = Ablehnung; leeres Ergebnis = Warnung (kein Fehler)

Erst wenn alles besteht, wird das Paket dauerhaft gespeichert (atomar: temporär aufbauen, dann verschieben).

## 4. Hinweise für die Kundendoku

- Beispielpaket als Download mitliefern
- Typische Fehler mit Meldungstext und Behebung auflisten
- Hinweis: `###TRENN###` darf in keinem Feldtext vorkommen
- Hinweis zum SQL-Dialekt (SQLite) mit Beispielen für übliche Abweichungen
- Datensicherung: Datenordner kopieren (Pfad je Betriebssystem angeben)
- Hinweis zum Packen: Zeilenenden CRLF/LF sind egal; ZIP mit gängigen Werkzeugen erzeugen, Dateinamen genau wie oben schreiben

## 5. Offen

- Menschliche Entscheidung zu allen Empfehlungen in Abschnitt 2 (inkl. „genau 25“ als harte Grenze)
- Freigabe durch den Kunden, bevor Implementierung und Inhaltserstellung starten
