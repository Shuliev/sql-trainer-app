-- Datenbank Stadtbibliothek
-- fuer SQLite

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS ausleihe;
DROP TABLE IF EXISTS mitglied;
DROP TABLE IF EXISTS buch;
DROP TABLE IF EXISTS autor;

-- Tabellen anlegen

CREATE TABLE autor (
  autor_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  land TEXT NOT NULL
);

CREATE TABLE buch (
  buch_id INTEGER PRIMARY KEY,
  titel TEXT NOT NULL,
  autor_id INTEGER NOT NULL,
  genre TEXT NOT NULL,
  erscheinungsjahr INTEGER NOT NULL,
  seiten INTEGER NOT NULL,
  FOREIGN KEY (autor_id) REFERENCES autor(autor_id)
);

CREATE TABLE mitglied (
  mitglied_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  wohnort TEXT NOT NULL,
  beitrittsdatum TEXT NOT NULL
);

CREATE TABLE ausleihe (
  ausleihe_id INTEGER PRIMARY KEY,
  buch_id INTEGER NOT NULL,
  mitglied_id INTEGER NOT NULL,
  ausleihdatum TEXT NOT NULL,
  rueckgabedatum TEXT,
  FOREIGN KEY (buch_id) REFERENCES buch(buch_id),
  FOREIGN KEY (mitglied_id) REFERENCES mitglied(mitglied_id)
);


-- Testdaten einfuegen

INSERT INTO autor VALUES
(1, 'Theodor', 'Fontane', 'Deutschland'),
(2, 'Franz', 'Kafka', 'Österreich-Ungarn'),
(3, 'Agatha', 'Christie', 'England'),
(4, 'Jane', 'Austen', 'England'),
(5, 'Thomas', 'Mann', 'Deutschland'),
(6, 'Jules', 'Verne', 'Frankreich'),
(7, 'Hermann', 'Hesse', 'Deutschland'),
(8, 'Arthur Conan', 'Doyle', 'Schottland');

INSERT INTO buch VALUES
(1, 'Effi Briest', 1, 'Roman', 1895, 320),
(2, 'Der Stechlin', 1, 'Roman', 1898, 480),
(3, 'Der Process', 2, 'Roman', 1925, 288),
(4, 'Die Verwandlung', 2, 'Erzählung', 1915, 96),
(5, 'Mord im Orient-Express', 3, 'Krimi', 1934, 256),
(6, 'Tod auf dem Nil', 3, 'Krimi', 1937, 304),
(7, 'Stolz und Vorurteil', 4, 'Roman', 1813, 432),
(8, 'Emma', 4, 'Roman', 1815, 512),
(9, 'Buddenbrooks', 5, 'Roman', 1901, 760),
(10, 'Der Zauberberg', 5, 'Roman', 1924, 1008),
(11, 'Reise um die Erde in 80 Tagen', 6, 'Abenteuer', 1873, 272),
(12, '20.000 Meilen unter dem Meer', 6, 'Abenteuer', 1870, 496),
(13, 'Der Steppenwolf', 7, 'Roman', 1927, 280),
(14, 'Siddhartha', 7, 'Erzählung', 1922, 128),
(15, 'Der Hund der Baskervilles', 8, 'Krimi', 1902, 240),
(16, 'Eine Studie in Scharlachrot', 8, 'Krimi', 1887, 176);

INSERT INTO mitglied VALUES
(1, 'Laura', 'Kaiser', 'Essen', '2021-03-15'),
(2, 'Tim', 'Krüger', 'Mülheim', '2021-08-01'),
(3, 'Nina', 'Lange', 'Essen', '2022-01-20'),
(4, 'David', 'Hartmann', 'Oberhausen', '2022-06-11'),
(5, 'Julia', 'Schwarz', 'Essen', '2022-11-03'),
(6, 'Felix', 'Zimmermann', 'Mülheim', '2023-02-17'),
(7, 'Lena', 'Braun', 'Bottrop', '2023-09-09'),
(8, 'Max', 'Hofmann', 'Essen', '2024-04-22'),
(9, 'Clara', 'Schröder', 'Oberhausen', '2025-01-30'),
(10, 'Noah', 'Neumann', 'Essen', '2025-07-14');

INSERT INTO ausleihe VALUES
(1, 7, 1, '2025-02-03', '2025-03-03'),
(2, 12, 3, '2025-05-09', '2025-06-09'),
(3, 12, 4, '2025-10-02', '2025-11-02'),
(4, 9, 3, '2025-07-21', '2025-08-21'),
(5, 11, 6, '2025-09-15', '2025-10-15'),
(6, 1, 5, '2025-01-28', '2025-02-28'),
(7, 9, 6, '2025-08-11', '2025-09-11'),
(8, 11, 7, '2025-09-06', '2025-10-06'),
(9, 1, 3, '2025-04-08', '2025-05-08'),
(10, 4, 3, '2025-06-06', '2025-07-06'),
(11, 11, 6, '2025-09-22', '2025-10-22'),
(12, 9, 3, '2025-08-26', '2025-09-26'),
(13, 7, 6, '2025-10-12', '2025-11-12'),
(14, 9, 8, '2025-03-25', '2025-04-25'),
(15, 10, 8, '2025-09-08', '2025-10-08'),
(16, 11, 5, '2025-08-17', '2025-09-17'),
(17, 7, 6, '2025-08-15', '2025-09-15'),
(18, 5, 8, '2025-08-22', '2025-09-22'),
(19, 6, 6, '2025-03-20', '2025-04-20'),
(20, 15, 8, '2025-05-10', '2025-06-10'),
(21, 5, 7, '2025-05-24', '2025-06-24'),
(22, 13, 8, '2025-09-12', '2025-10-12'),
(23, 1, 2, '2025-06-24', '2025-07-24'),
(24, 12, 4, '2025-02-02', '2025-03-02'),
(25, 5, 1, '2025-05-19', '2025-06-19'),
(26, 6, 2, '2025-09-05', NULL),
(27, 9, 4, '2025-04-02', NULL),
(28, 7, 1, '2025-01-12', NULL),
(29, 1, 3, '2025-04-22', NULL),
(30, 1, 2, '2025-02-03', NULL);
