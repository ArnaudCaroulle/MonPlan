-- MonPlan - schéma initial MySQL sans contraintes de clés étrangères.
CREATE DATABASE IF NOT EXISTS monplan CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE monplan;

CREATE TABLE IF NOT EXISTS utilisateurs (
  Id varchar(255) NOT NULL PRIMARY KEY,
  UserName varchar(256) NULL,
  NormalizedUserName varchar(256) NULL,
  Email varchar(256) NULL,
  NormalizedEmail varchar(256) NULL,
  EmailConfirmed tinyint(1) NOT NULL DEFAULT 0,
  PasswordHash longtext NULL,
  SecurityStamp longtext NULL,
  ConcurrencyStamp longtext NULL,
  PhoneNumber longtext NULL,
  PhoneNumberConfirmed tinyint(1) NOT NULL DEFAULT 0,
  TwoFactorEnabled tinyint(1) NOT NULL DEFAULT 0,
  LockoutEnd datetime(6) NULL,
  LockoutEnabled tinyint(1) NOT NULL DEFAULT 0,
  AccessFailedCount int NOT NULL DEFAULT 0,
  Prenom varchar(80) NULL,
  Nom varchar(80) NULL,
  FuseauHoraire varchar(80) NOT NULL DEFAULT 'Europe/Paris',
  DateCreationUtc datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  KEY EmailIndex (NormalizedEmail),
  UNIQUE KEY UserNameIndex (NormalizedUserName)
);

CREATE TABLE IF NOT EXISTS roles (
  Id varchar(255) NOT NULL PRIMARY KEY,
  Name varchar(256) NULL,
  NormalizedName varchar(256) NULL,
  ConcurrencyStamp longtext NULL,
  UNIQUE KEY RoleNameIndex (NormalizedName)
);

CREATE TABLE IF NOT EXISTS utilisateurs_roles (UserId varchar(255) NOT NULL, RoleId varchar(255) NOT NULL, PRIMARY KEY (UserId, RoleId));
CREATE TABLE IF NOT EXISTS utilisateurs_revendications (Id int NOT NULL AUTO_INCREMENT PRIMARY KEY, UserId varchar(255) NOT NULL, ClaimType longtext NULL, ClaimValue longtext NULL, KEY IX_UserId (UserId));
CREATE TABLE IF NOT EXISTS utilisateurs_connexions (LoginProvider varchar(255) NOT NULL, ProviderKey varchar(255) NOT NULL, ProviderDisplayName longtext NULL, UserId varchar(255) NOT NULL, PRIMARY KEY (LoginProvider, ProviderKey), KEY IX_UserId (UserId));
CREATE TABLE IF NOT EXISTS roles_revendications (Id int NOT NULL AUTO_INCREMENT PRIMARY KEY, RoleId varchar(255) NOT NULL, ClaimType longtext NULL, ClaimValue longtext NULL, KEY IX_RoleId (RoleId));
CREATE TABLE IF NOT EXISTS utilisateurs_jetons (UserId varchar(255) NOT NULL, LoginProvider varchar(255) NOT NULL, Name varchar(255) NOT NULL, Value longtext NULL, PRIMARY KEY (UserId, LoginProvider, Name));
