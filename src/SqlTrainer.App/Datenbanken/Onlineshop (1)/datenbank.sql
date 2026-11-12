-- Datenbank Onlineshop
-- fuer SQLite

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS bestellposition;
DROP TABLE IF EXISTS bestellung;
DROP TABLE IF EXISTS produkt;
DROP TABLE IF EXISTS kategorie;
DROP TABLE IF EXISTS kunde;

-- Tabellen anlegen

CREATE TABLE kunde (
  kunde_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  stadt TEXT NOT NULL,
  email TEXT NOT NULL UNIQUE,
  registriert_am TEXT NOT NULL
);

CREATE TABLE kategorie (
  kategorie_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL UNIQUE
);

CREATE TABLE produkt (
  produkt_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  kategorie_id INTEGER NOT NULL,
  preis REAL NOT NULL,
  lagerbestand INTEGER NOT NULL,
  FOREIGN KEY (kategorie_id) REFERENCES kategorie(kategorie_id)
);

CREATE TABLE bestellung (
  bestellung_id INTEGER PRIMARY KEY,
  kunde_id INTEGER NOT NULL,
  bestelldatum TEXT NOT NULL,
  status TEXT NOT NULL,
  FOREIGN KEY (kunde_id) REFERENCES kunde(kunde_id)
);

CREATE TABLE bestellposition (
  position_id INTEGER PRIMARY KEY,
  bestellung_id INTEGER NOT NULL,
  produkt_id INTEGER NOT NULL,
  menge INTEGER NOT NULL,
  einzelpreis REAL NOT NULL,
  FOREIGN KEY (bestellung_id) REFERENCES bestellung(bestellung_id),
  FOREIGN KEY (produkt_id) REFERENCES produkt(produkt_id)
);


-- Testdaten einfuegen

INSERT INTO kunde VALUES
(1, 'Anna', 'Schmidt', 'Essen', 'anna.schmidt@web.de', '2023-02-14'),
(2, 'Lukas', 'Müller', 'Dortmund', 'lukas.mueller@gmx.de', '2023-03-02'),
(3, 'Mia', 'Weber', 'Essen', 'mia.weber@gmail.com', '2023-05-19'),
(4, 'Jonas', 'Fischer', 'Bochum', 'j.fischer@web.de', '2023-07-08'),
(5, 'Lea', 'Wagner', 'Köln', 'lea.wagner@t-online.de', '2023-09-23'),
(6, 'Paul', 'Becker', 'Düsseldorf', 'paul.becker@gmail.com', '2023-11-30'),
(7, 'Emma', 'Hoffmann', 'Essen', 'emma.h@gmx.de', '2024-01-12'),
(8, 'Ben', 'Schulz', 'Dortmund', 'ben.schulz@web.de', '2024-02-27'),
(9, 'Sophie', 'Koch', 'Köln', 'sophie.koch@gmail.com', '2024-04-05'),
(10, 'Finn', 'Richter', 'Bochum', 'finn.richter@gmx.de', '2024-06-18'),
(11, 'Hannah', 'Klein', 'Gelsenkirchen', 'hannah.klein@web.de', '2024-08-03'),
(12, 'Elias', 'Wolf', 'Essen', 'elias.wolf@t-online.de', '2024-10-21');

INSERT INTO kategorie VALUES
(1, 'Elektronik'),
(2, 'Bücher'),
(3, 'Haushalt'),
(4, 'Sport'),
(5, 'Spielwaren');

INSERT INTO produkt VALUES
(1, 'Bluetooth-Kopfhörer', 1, 79.99, 35),
(2, 'USB-C-Ladegerät', 1, 24.9, 120),
(3, 'Smartwatch', 1, 199.0, 12),
(4, 'Tablet 10 Zoll', 1, 329.0, 8),
(5, 'SQL für Einsteiger', 2, 29.95, 40),
(6, 'Kochbuch Vegetarisch', 2, 19.99, 25),
(7, 'Krimi: Nebel über dem Hafen', 2, 12.5, 60),
(8, 'Kaffeemaschine', 3, 89.0, 15),
(9, 'Wasserkocher', 3, 34.99, 30),
(10, 'Messerset', 3, 59.9, 18),
(11, 'Yogamatte', 4, 27.99, 45),
(12, 'Hantelset 20 kg', 4, 64.5, 10),
(13, 'Fußball', 4, 22.0, 50),
(14, 'Brettspiel Stadtplaner', 5, 39.99, 22),
(15, 'Holzeisenbahn', 5, 49.0, 0);

INSERT INTO bestellung VALUES
(1, 3, '2025-10-28', 'offen'),
(2, 7, '2025-04-04', 'versendet'),
(3, 10, '2025-01-23', 'versendet'),
(4, 6, '2025-01-01', 'offen'),
(5, 1, '2025-09-08', 'versendet'),
(6, 4, '2025-08-10', 'offen'),
(7, 5, '2025-02-24', 'versendet'),
(8, 5, '2025-05-19', 'versendet'),
(9, 7, '2025-07-22', 'bezahlt'),
(10, 6, '2025-02-15', 'storniert'),
(11, 1, '2025-08-02', 'versendet'),
(12, 3, '2025-09-08', 'offen'),
(13, 4, '2025-07-17', 'versendet'),
(14, 1, '2025-07-26', 'storniert'),
(15, 1, '2025-08-28', 'versendet'),
(16, 7, '2025-06-01', 'versendet'),
(17, 4, '2025-11-06', 'versendet'),
(18, 2, '2025-02-28', 'offen'),
(19, 2, '2025-10-06', 'versendet'),
(20, 9, '2025-03-22', 'versendet'),
(21, 2, '2025-12-30', 'offen');

INSERT INTO bestellposition VALUES
(1, 1, 2, 2, 24.9),
(2, 1, 8, 2, 89.0),
(3, 2, 7, 2, 12.5),
(4, 3, 12, 3, 64.5),
(5, 3, 4, 1, 329.0),
(6, 4, 9, 3, 34.99),
(7, 4, 1, 1, 79.99),
(8, 4, 7, 2, 12.5),
(9, 5, 9, 2, 34.99),
(10, 5, 4, 1, 329.0),
(11, 6, 9, 1, 34.99),
(12, 6, 11, 1, 27.99),
(13, 7, 12, 3, 64.5),
(14, 7, 9, 3, 34.99),
(15, 7, 7, 1, 12.5),
(16, 8, 7, 2, 12.5),
(17, 8, 10, 1, 59.9),
(18, 8, 1, 3, 79.99),
(19, 9, 9, 3, 34.99),
(20, 9, 12, 3, 64.5),
(21, 10, 2, 2, 24.9),
(22, 10, 3, 2, 199.0),
(23, 10, 9, 2, 34.99),
(24, 11, 10, 2, 59.9),
(25, 11, 13, 3, 22.0),
(26, 11, 12, 1, 64.5),
(27, 12, 9, 3, 34.99),
(28, 13, 6, 3, 19.99),
(29, 13, 8, 3, 89.0),
(30, 13, 5, 3, 29.95),
(31, 14, 13, 3, 22.0),
(32, 14, 3, 1, 199.0),
(33, 14, 9, 2, 34.99),
(34, 15, 9, 2, 34.99),
(35, 15, 4, 2, 329.0),
(36, 15, 13, 2, 22.0),
(37, 16, 10, 2, 59.9),
(38, 16, 13, 3, 22.0),
(39, 16, 6, 1, 19.99),
(40, 17, 3, 2, 199.0),
(41, 17, 2, 1, 24.9),
(42, 17, 9, 3, 34.99),
(43, 18, 1, 1, 79.99),
(44, 18, 5, 2, 29.95),
(45, 19, 2, 1, 24.9),
(46, 19, 3, 2, 199.0),
(47, 20, 12, 3, 64.5),
(48, 20, 5, 2, 29.95),
(49, 20, 8, 2, 89.0),
(50, 21, 3, 1, 199.0);
