-- Datenbank Fuhrpark
-- fuer SQLite

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS wartung;
DROP TABLE IF EXISTS fahrt;
DROP TABLE IF EXISTS fahrzeug;
DROP TABLE IF EXISTS mitarbeiter;
DROP TABLE IF EXISTS abteilung;

-- Tabellen anlegen

CREATE TABLE abteilung (
  abteilung_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL UNIQUE
);

CREATE TABLE mitarbeiter (
  mitarbeiter_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  fuehrerschein TEXT NOT NULL,
  abteilung_id INTEGER NOT NULL,
  FOREIGN KEY (abteilung_id) REFERENCES abteilung(abteilung_id)
);

CREATE TABLE fahrzeug (
  fahrzeug_id INTEGER PRIMARY KEY,
  kennzeichen TEXT NOT NULL UNIQUE,
  marke TEXT NOT NULL,
  modell TEXT NOT NULL,
  baujahr INTEGER NOT NULL,
  kilometerstand INTEGER NOT NULL,
  kraftstoff TEXT NOT NULL
);

CREATE TABLE fahrt (
  fahrt_id INTEGER PRIMARY KEY,
  fahrzeug_id INTEGER NOT NULL,
  mitarbeiter_id INTEGER NOT NULL,
  datum TEXT NOT NULL,
  kilometer INTEGER NOT NULL,
  zweck TEXT NOT NULL,
  FOREIGN KEY (fahrzeug_id) REFERENCES fahrzeug(fahrzeug_id),
  FOREIGN KEY (mitarbeiter_id) REFERENCES mitarbeiter(mitarbeiter_id)
);

CREATE TABLE wartung (
  wartung_id INTEGER PRIMARY KEY,
  fahrzeug_id INTEGER NOT NULL,
  datum TEXT NOT NULL,
  art TEXT NOT NULL,
  kosten REAL NOT NULL,
  FOREIGN KEY (fahrzeug_id) REFERENCES fahrzeug(fahrzeug_id)
);


-- Testdaten einfuegen

INSERT INTO abteilung VALUES
(1, 'Vertrieb'),
(2, 'Logistik'),
(3, 'Kundendienst'),
(4, 'Verwaltung');

INSERT INTO mitarbeiter VALUES
(1, 'Markus', 'Ernst', 'B', 1),
(2, 'Petra', 'Schuster', 'B', 1),
(3, 'Kemal', 'Aydin', 'C1', 2),
(4, 'Jan', 'Kuhn', 'C1', 2),
(5, 'Olga', 'Smirnova', 'B', 2),
(6, 'Sven', 'Berger', 'B', 3),
(7, 'Nadine', 'Lang', 'B', 3),
(8, 'Rolf', 'Seidel', 'B', 4),
(9, 'Ute', 'Fuchs', 'B', 4);

INSERT INTO fahrzeug VALUES
(1, 'E-RL 101', 'VW', 'Golf', 2019, 84500, 'Benzin'),
(2, 'E-RL 102', 'VW', 'Passat', 2020, 121300, 'Diesel'),
(3, 'E-RL 201', 'Mercedes-Benz', 'Sprinter', 2018, 198700, 'Diesel'),
(4, 'E-RL 202', 'Ford', 'Transit', 2021, 76400, 'Diesel'),
(5, 'E-RL 103', 'Opel', 'Corsa-e', 2022, 31200, 'Elektro'),
(6, 'E-RL 104', 'Skoda', 'Octavia', 2017, 156900, 'Diesel'),
(7, 'E-RL 105', 'Toyota', 'Corolla', 2023, 18300, 'Hybrid'),
(8, 'E-RL 203', 'Renault', 'Kangoo E-Tech', 2024, 6200, 'Elektro');

INSERT INTO fahrt VALUES
(1, 3, 2, '2025-02-24', 214, 'Servicetermin'),
(2, 1, 2, '2025-02-01', 217, 'Materialabholung'),
(3, 7, 3, '2025-01-08', 278, 'Materialabholung'),
(4, 3, 3, '2025-03-27', 66, 'Messe'),
(5, 1, 2, '2025-05-26', 151, 'Auslieferung'),
(6, 3, 2, '2025-05-21', 386, 'Messe'),
(7, 7, 1, '2025-10-11', 355, 'Servicetermin'),
(8, 2, 5, '2025-03-08', 254, 'Messe'),
(9, 7, 1, '2025-09-27', 165, 'Kundentermin'),
(10, 5, 3, '2025-05-28', 403, 'Materialabholung'),
(11, 4, 2, '2025-07-20', 159, 'Servicetermin'),
(12, 2, 4, '2025-04-10', 144, 'Kundentermin'),
(13, 1, 1, '2025-08-21', 155, 'Materialabholung'),
(14, 6, 5, '2025-08-23', 187, 'Auslieferung'),
(15, 2, 6, '2025-02-14', 115, 'Schulung'),
(16, 4, 6, '2025-05-06', 194, 'Servicetermin'),
(17, 5, 6, '2025-06-21', 297, 'Auslieferung'),
(18, 1, 3, '2025-01-23', 129, 'Messe'),
(19, 5, 7, '2025-10-28', 133, 'Kundentermin'),
(20, 2, 3, '2025-05-15', 25, 'Kundentermin'),
(21, 6, 3, '2025-02-10', 388, 'Schulung'),
(22, 1, 3, '2025-06-10', 176, 'Auslieferung'),
(23, 6, 7, '2025-07-28', 329, 'Schulung'),
(24, 1, 7, '2025-05-20', 110, 'Servicetermin'),
(25, 2, 3, '2025-05-13', 318, 'Auslieferung'),
(26, 5, 3, '2025-01-12', 34, 'Servicetermin'),
(27, 3, 2, '2025-06-10', 304, 'Kundentermin'),
(28, 2, 4, '2025-07-07', 70, 'Kundentermin');

INSERT INTO wartung VALUES
(1, 1, '2025-02-11', 'Inspektion', 389.9),
(2, 2, '2025-03-04', 'Inspektion', 452.0),
(3, 3, '2025-01-20', 'Reifenwechsel', 210.0),
(4, 3, '2025-04-15', 'Bremsen', 780.5),
(5, 3, '2025-08-02', 'Inspektion', 615.0),
(6, 4, '2025-05-27', 'Inspektion', 498.3),
(7, 6, '2025-03-18', 'Kupplung', 1240.0),
(8, 6, '2025-06-09', 'Reifenwechsel', 185.0),
(9, 6, '2025-09-22', 'Inspektion', 430.0),
(10, 2, '2025-10-01', 'Reifenwechsel', 195.0),
(11, 5, '2025-07-14', 'Inspektion', 160.0),
(12, 1, '2025-10-20', 'Reifenwechsel', 175.0);
