-- MonPlan - catalogue sportif, saisons et plan utilisateur.
-- Mise à jour d'une base existante. Ce script suit la convention du projet :
-- relations logiques indexées, sans contraintes de clés étrangères en base.
USE monplan;

CREATE TABLE IF NOT EXISTS manifestations_sportives (
  Id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  Nom varchar(200) NOT NULL,
  Ville varchar(120) NOT NULL,
  CodePostal varchar(20) NULL,
  Pays varchar(100) NOT NULL DEFAULT 'France',
  DateDebut date NOT NULL,
  DateFin date NOT NULL,
  Description varchar(4000) NULL,
  UrlSiteOfficiel varchar(500) NULL,
  EstActive tinyint(1) NOT NULL DEFAULT 1,
  DateCreation datetime(6) NOT NULL,
  DateModification datetime(6) NULL,
  KEY IX_manifestations_actives_dates (EstActive, DateDebut, DateFin)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS epreuves_sportives (
  Id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  ManifestationSportiveId int NOT NULL,
  Nom varchar(200) NOT NULL,
  Discipline int NOT NULL,
  Format varchar(100) NULL,
  DescriptionDistance varchar(1000) NULL,
  DateEpreuve date NOT NULL,
  HeureDepart time(6) NULL,
  EstActive tinyint(1) NOT NULL DEFAULT 1,
  DateCreation datetime(6) NOT NULL,
  DateModification datetime(6) NULL,
  KEY IX_epreuves_manifestation (ManifestationSportiveId),
  KEY IX_epreuves_date (DateEpreuve),
  KEY IX_epreuves_filtres (EstActive, Discipline, DateEpreuve)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS saisons_sportives (
  Id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  ApplicationUserId varchar(255) NOT NULL,
  Nom varchar(200) NOT NULL,
  DateDebut date NOT NULL,
  DateFin date NOT NULL,
  DateCreation datetime(6) NOT NULL,
  DateModification datetime(6) NULL,
  KEY IX_saisons_utilisateur_date (ApplicationUserId, DateDebut)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS participations_utilisateurs (
  Id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  ApplicationUserId varchar(255) NOT NULL,
  EpreuveSportiveId int NOT NULL,
  SaisonSportiveId int NOT NULL,
  StatutParticipation int NOT NULL,
  DateCreation datetime(6) NOT NULL,
  DateModification datetime(6) NULL,
  UNIQUE KEY UX_participations_utilisateur_epreuve (ApplicationUserId, EpreuveSportiveId),
  KEY IX_participations_saison (SaisonSportiveId),
  KEY IX_participations_epreuve (EpreuveSportiveId)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
