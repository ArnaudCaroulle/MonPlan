# MonPlan

MonPlan est une application web ASP.NET Core MVC en français destinée à construire et visualiser une saison sportive sur une timeline annuelle.

## Socle technique

- .NET 8 / ASP.NET Core MVC
- ASP.NET Core Identity pour l'inscription, la connexion, la confirmation email, la récupération de mot de passe, les rôles et autorisations
- Entity Framework Core avec provider MySQL Pomelo
- Base suggérée : `monplan`
- Culture par défaut : `fr-FR`
- Fuseau horaire métier : `Europe/Paris`

## Configuration

La chaîne de connexion fournie est un exemple local non sensible. Pour IONOS ou la production, utiliser des variables d'environnement ou un secret applicatif hors dépôt :

```bash
ConnectionStrings__DefaultConnection="Server=...;Database=monplan;User=...;Password=...;TreatTinyAsBoolean=true;"
```

## Base de données

Les scripts SQL sont disponibles dans `scripts/`. Ils évitent volontairement les contraintes de clés étrangères afin de respecter la convention projet.

## Commandes utiles

```bash
dotnet restore MonPlan.sln
dotnet build MonPlan.sln
dotnet test MonPlan.sln
```

## Déploiement IONOS

1. Créer une base MySQL `monplan`.
2. Exécuter `scripts/001_schema_identity_mysql.sql`, puis `scripts/002_seed_roles.sql`.
3. Publier l'application en Release.
4. Injecter la chaîne de connexion par variable d'environnement.
5. Configurer HTTPS et le domaine public.

## Sécurité du dépôt public

Ne jamais versionner de secrets SMTP, API, certificats privés, chaînes de connexion réelles ou données utilisateur.
