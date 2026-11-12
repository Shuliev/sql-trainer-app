-- Datenbank Schulverwaltung
-- fuer SQLite

PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS note;
DROP TABLE IF EXISTS fach;
DROP TABLE IF EXISTS schueler;
DROP TABLE IF EXISTS klasse;
DROP TABLE IF EXISTS lehrer;

-- Tabellen anlegen

CREATE TABLE lehrer (
  lehrer_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  kuerzel TEXT NOT NULL UNIQUE
);

CREATE TABLE klasse (
  klasse_id INTEGER PRIMARY KEY,
  bezeichnung TEXT NOT NULL UNIQUE,
  raum TEXT NOT NULL,
  klassenlehrer_id INTEGER NOT NULL,
  FOREIGN KEY (klassenlehrer_id) REFERENCES lehrer(lehrer_id)
);

CREATE TABLE schueler (
  schueler_id INTEGER PRIMARY KEY,
  vorname TEXT NOT NULL,
  nachname TEXT NOT NULL,
  geburtsdatum TEXT NOT NULL,
  klasse_id INTEGER NOT NULL,
  FOREIGN KEY (klasse_id) REFERENCES klasse(klasse_id)
);

CREATE TABLE fach (
  fach_id INTEGER PRIMARY KEY,
  name TEXT NOT NULL UNIQUE,
  lehrer_id INTEGER NOT NULL,
  FOREIGN KEY (lehrer_id) REFERENCES lehrer(lehrer_id)
);

CREATE TABLE note (
  note_id INTEGER PRIMARY KEY,
  schueler_id INTEGER NOT NULL,
  fach_id INTEGER NOT NULL,
  note INTEGER NOT NULL CHECK (note BETWEEN 1 AND 6),
  datum TEXT NOT NULL,
  FOREIGN KEY (schueler_id) REFERENCES schueler(schueler_id),
  FOREIGN KEY (fach_id) REFERENCES fach(fach_id)
);


-- Testdaten einfuegen

INSERT INTO lehrer VALUES
(1, 'Sabine', 'Vogel', 'VOG'),
(2, 'Thomas', 'Brandt', 'BRA'),
(3, 'Kerstin', 'Lorenz', 'LOR'),
(4, 'Michael', 'Peters', 'PET'),
(5, 'Andrea', 'Busch', 'BUS'),
(6, 'Stefan', 'Graf', 'GRA');

INSERT INTO klasse VALUES
(1, 'FI24A', 'A101', 1),
(2, 'FI24B', 'A102', 2),
(3, 'FI25A', 'B201', 3),
(4, 'KB25A', 'B202', 4);

INSERT INTO schueler VALUES
(1, 'Leon', 'Arslan', '2008-04-19', 1),
(2, 'Marie', 'Becker', '2007-09-05', 2),
(3, 'Emil', 'Demir', '2006-06-20', 3),
(4, 'Lina', 'Engel', '2005-08-21', 4),
(5, 'Noah', 'Franke', '2008-10-03', 1),
(6, 'Ella', 'Günther', '2007-10-01', 2),
(7, 'Luis', 'Haas', '2006-08-09', 3),
(8, 'Ida', 'Jung', '2005-09-08', 4),
(9, 'Henry', 'Kraus', '2008-04-23', 1),
(10, 'Mila', 'Lehmann', '2007-08-18', 2),
(11, 'Anton', 'Möller', '2006-09-16', 3),
(12, 'Lia', 'Nowak', '2005-07-21', 4),
(13, 'Theo', 'Otto', '2008-03-08', 1),
(14, 'Frieda', 'Pohl', '2007-11-05', 2),
(15, 'Jakob', 'Roth', '2006-09-13', 3),
(16, 'Greta', 'Sauer', '2005-12-01', 4),
(17, 'Oskar', 'Thiel', '2008-11-25', 1),
(18, 'Romy', 'Ulrich', '2007-02-06', 2),
(19, 'Karl', 'Voigt', '2006-10-02', 3),
(20, 'Nele', 'Yilmaz', '2005-05-25', 4);

INSERT INTO fach VALUES
(1, 'Anwendungsentwicklung', 1),
(2, 'Datenbanken', 1),
(3, 'Netzwerktechnik', 2),
(4, 'Deutsch', 3),
(5, 'Englisch', 4),
(6, 'Wirtschaft', 5),
(7, 'Sport', 6);

INSERT INTO note VALUES
(1, 1, 2, 4, '2025-12-23'),
(2, 1, 1, 4, '2025-12-24'),
(3, 1, 4, 6, '2025-12-05'),
(4, 2, 2, 1, '2025-10-16'),
(5, 2, 4, 3, '2025-11-22'),
(6, 2, 1, 4, '2025-11-14'),
(7, 3, 2, 6, '2025-11-18'),
(8, 3, 6, 6, '2025-12-19'),
(9, 3, 5, 3, '2025-11-22'),
(10, 4, 2, 6, '2025-10-23'),
(11, 4, 1, 3, '2025-09-23'),
(12, 4, 4, 3, '2025-11-10'),
(13, 5, 2, 4, '2025-12-03'),
(14, 5, 1, 3, '2025-09-14'),
(15, 5, 6, 2, '2025-09-10'),
(16, 6, 2, 2, '2025-09-20'),
(17, 6, 5, 6, '2025-09-13'),
(18, 6, 6, 6, '2025-11-18'),
(19, 7, 2, 1, '2025-11-01'),
(20, 7, 4, 2, '2025-09-20'),
(21, 7, 3, 5, '2025-09-07'),
(22, 8, 2, 6, '2025-11-05'),
(23, 8, 5, 1, '2025-11-11'),
(24, 8, 4, 3, '2025-10-28'),
(25, 9, 2, 4, '2025-12-21'),
(26, 9, 5, 6, '2025-09-20'),
(27, 9, 6, 5, '2025-11-14'),
(28, 10, 2, 4, '2025-11-17'),
(29, 10, 3, 3, '2025-11-01'),
(30, 10, 4, 4, '2025-11-01'),
(31, 11, 2, 1, '2025-11-15'),
(32, 11, 5, 3, '2025-11-20'),
(33, 11, 3, 3, '2025-12-01'),
(34, 12, 2, 1, '2025-11-09'),
(35, 12, 6, 4, '2025-11-19'),
(36, 12, 1, 6, '2025-11-06'),
(37, 13, 2, 3, '2025-11-28'),
(38, 13, 4, 6, '2025-11-10'),
(39, 13, 3, 4, '2025-09-25'),
(40, 14, 2, 3, '2025-10-21'),
(41, 14, 1, 3, '2025-10-11'),
(42, 14, 3, 2, '2025-12-21'),
(43, 15, 2, 6, '2025-11-11'),
(44, 15, 1, 3, '2025-12-26'),
(45, 15, 6, 2, '2025-09-11'),
(46, 16, 2, 3, '2025-10-26'),
(47, 16, 3, 2, '2025-09-17'),
(48, 16, 5, 3, '2025-11-26'),
(49, 17, 2, 3, '2025-11-26'),
(50, 17, 6, 2, '2025-11-19'),
(51, 17, 3, 2, '2025-12-10'),
(52, 18, 2, 4, '2025-11-21'),
(53, 18, 6, 4, '2025-11-14'),
(54, 18, 4, 6, '2025-12-02');
