USE monplan;
INSERT INTO roles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (UUID(), 'Administrateur', 'ADMINISTRATEUR', UUID()), (UUID(), 'Utilisateur', 'UTILISATEUR', UUID())
ON DUPLICATE KEY UPDATE Name = VALUES(Name);
