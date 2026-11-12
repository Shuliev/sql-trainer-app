-- Datenbank Kino
-- fuer SQLite

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS ticket;
DROP TABLE IF EXISTS kunde;
DROP TABLE IF EXISTS vorstellung;
DROP TABLE IF EXISTS saal;
DROP TABLE IF EXISTS film;

-- Tabellen anlegen

CREATE TABLE film (
  film_id INTEGER PRIMARY KEY,
  titel TEXT NOT NULL,
  genre TEXT NOT NULL,
  laufzeit_min INTEGER NOT NULL,
  fsk INTEGER NOT NULL
);

CREATE TABLE saal (
  saal_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  plaetze INTEGER NOT NULL
);

CREATE TABLE vorstellung (
  vorstellung_id INTEGER PRIMARY KEY,
  film_id INTEGER NOT NULL,
  saal_id INTEGER NOT NULL,
  datum TEXT NOT NULL,
  uhrzeit TEXT NOT NULL,
  preis REAL NOT NULL,
  FOREIGN KEY (film_id) REFERENCES film(film_id),
  FOREIGN KEY (saal_id) REFERENCES saal(saal_id)
);

CREATE TABLE kunde (
  kunde_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  geburtsjahr INTEGER NOT NULL,
  wohnort TEXT NOT NULL
);

CREATE TABLE ticket (
  ticket_id INTEGER PRIMARY KEY,
  vorstellung_id INTEGER NOT NULL,
  kunde_id INTEGER NOT NULL,
  sitzplatz TEXT NOT NULL,
  FOREIGN KEY (vorstellung_id) REFERENCES vorstellung(vorstellung_id),
  FOREIGN KEY (kunde_id) REFERENCES kunde(kunde_id)
);


-- Testdaten einfuegen

INSERT INTO film VALUES
(1, 'Schatten über Bochum', 'Thriller', 118, 16),
(2, 'Die Reise des kleinen Kometen', 'Animation', 92, 0),
(3, 'Kohle und Sterne', 'Drama', 134, 12),
(4, 'Gelächter im Treppenhaus', 'Komödie', 101, 6),
(5, 'Projekt Nordlicht', 'Science-Fiction', 146, 12),
(6, 'Das letzte Signal', 'Thriller', 109, 16),
(7, 'Omas Geheimrezept', 'Komödie', 95, 0),
(8, 'Nachtschicht', 'Horror', 99, 18),
(9, 'Wellen der Zeit', 'Drama', 127, 12);

INSERT INTO saal VALUES
(1, 'Saal Förderturm', 180),
(2, 'Saal Kaue', 90),
(3, 'Saal Lampenstube', 45);

INSERT INTO vorstellung VALUES
(1, 5, 2, '2025-11-25', '14:30', 7.5),
(2, 4, 1, '2025-11-12', '17:00', 11.0),
(3, 8, 3, '2025-11-04', '22:45', 11.0),
(4, 4, 2, '2025-11-09', '17:00', 9.5),
(5, 3, 1, '2025-11-25', '22:45', 11.0),
(6, 2, 1, '2025-11-05', '22:45', 9.5),
(7, 1, 1, '2025-11-25', '14:30', 7.5),
(8, 4, 1, '2025-11-10', '17:00', 11.0),
(9, 6, 1, '2025-11-23', '17:00', 11.0),
(10, 4, 2, '2025-11-01', '22:45', 11.0),
(11, 6, 2, '2025-11-03', '17:00', 9.5),
(12, 6, 3, '2025-11-22', '14:30', 7.5),
(13, 6, 2, '2025-11-16', '20:15', 9.5),
(14, 6, 3, '2025-11-06', '22:45', 9.5),
(15, 1, 3, '2025-11-12', '14:30', 7.5),
(16, 7, 2, '2025-11-13', '22:45', 9.5),
(17, 1, 3, '2025-11-07', '17:00', 9.5),
(18, 2, 2, '2025-11-17', '22:45', 11.0);

INSERT INTO kunde VALUES
(1, 'Selin', 'Kaya', 1998, 'Essen'),
(2, 'Moritz', 'Böhm', 1985, 'Bochum'),
(3, 'Johanna', 'Frank', 2001, 'Essen'),
(4, 'Erik', 'Hahn', 1979, 'Gelsenkirchen'),
(5, 'Merve', 'Öztürk', 1995, 'Essen'),
(6, 'Tobias', 'Simon', 1990, 'Bochum'),
(7, 'Carla', 'Vogt', 2004, 'Herne'),
(8, 'Dominik', 'Winter', 1988, 'Essen'),
(9, 'Pia', 'Albrecht', 1972, 'Bochum'),
(10, 'Yusuf', 'Celik', 2006, 'Herne');

INSERT INTO ticket VALUES
(1, 12, 5, 'H2'),
(2, 12, 5, 'A7'),
(3, 3, 4, 'F9'),
(4, 12, 3, 'F5'),
(5, 18, 2, 'E11'),
(6, 11, 5, 'C13'),
(7, 3, 3, 'E8'),
(8, 6, 1, 'B10'),
(9, 18, 7, 'A4'),
(10, 12, 5, 'H11'),
(11, 14, 3, 'A15'),
(12, 2, 8, 'F14'),
(13, 7, 3, 'C11'),
(14, 14, 2, 'C7'),
(15, 12, 3, 'A14'),
(16, 14, 5, 'C8'),
(17, 6, 8, 'H12'),
(18, 11, 8, 'E5'),
(19, 16, 7, 'C2'),
(20, 13, 3, 'H14'),
(21, 11, 3, 'B8'),
(22, 9, 6, 'B13'),
(23, 12, 1, 'E6'),
(24, 18, 5, 'H5'),
(25, 10, 6, 'C10'),
(26, 1, 8, 'E6'),
(27, 9, 8, 'E14'),
(28, 17, 6, 'F5'),
(29, 12, 7, 'F15'),
(30, 6, 8, 'F6'),
(31, 17, 3, 'C4'),
(32, 12, 8, 'E12'),
(33, 3, 7, 'C10'),
(34, 17, 7, 'E10'),
(35, 18, 5, 'A4'),
(36, 6, 8, 'C4'),
(37, 6, 1, 'H4'),
(38, 6, 1, 'C2'),
(39, 11, 3, 'H4'),
(40, 18, 1, 'G8'),
(41, 12, 7, 'B10'),
(42, 7, 4, 'F1'),
(43, 12, 7, 'E14'),
(44, 14, 2, 'F1'),
(45, 18, 5, 'B5');
