# MonPlan

MonPlan est une application web ASP.NET Core MVC en français destinée à construire un calendrier sportif personnel.

## Fonctionnalités sportives

- **Catalogue interne** : recherche serveur des épreuves actives par nom, format, ville, discipline et dates.
- **Détail d'une épreuve** : distinction entre la manifestation globale et l'épreuve sportive précise.
- **Mes saisons** : création et consultation des périodes sportives privées de l'utilisateur.
- **Ajout au plan** : association d'une épreuve à une saison compatible avec le statut « Envisagée » ou « Inscrit ».
- **Mon plan** : participations privées regroupées par saison et classées chronologiquement.

Pages principales : `/catalogue`, `/catalogue/epreuve/{id}`, `/mes-saisons`, `/mes-saisons/creer`, `/mon-plan`.

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

Les scripts SQL sont disponibles dans `scripts/`. Ils évitent volontairement les contraintes de clés étrangères afin de respecter la convention projet. Les relations restent contrôlées dans les services et indexées ; l'unicité utilisateur/épreuve est en plus protégée par un index unique.

Le dépôt utilise actuellement des **scripts SQL versionnés et non des migrations EF Core**. Il ne faut donc pas lancer `dotnet ef database update` sans décider explicitement d'une évolution de cette stratégie. Pour une base MySQL existante :

```bash
mysql -u UTILISATEUR -p monplan < scripts/003_catalogue_sportif_et_plan.sql
```

Pour une nouvelle base, exécuter dans l'ordre `001_schema_identity_mysql.sql`, `002_seed_roles.sql`, puis `003_catalogue_sportif_et_plan.sql`.

## Données de démonstration

En environnement `Development`, `ServiceInitialisationDonneesDemo` ajoute automatiquement, et uniquement lorsque le catalogue est vide, des données fictives portant toutes la mention « Démo » : 10 km, semi-marathon, trail 25 km, triathlon M et triathlon L. Il faut d'abord appliquer le script `003`. Aucun utilisateur, email, identifiant Identity fixe ou secret n'est créé.

## Commandes utiles

```bash
dotnet restore MonPlan.sln
dotnet build MonPlan.sln
dotnet test MonPlan.sln
```

## Limites de cette première version

- Le catalogue est interne et les entrées fournies sont exclusivement des données de démonstration, non officielles.
- Aucune synchronisation FFA ou FFTRI, aucun import automatique et aucun scraping ne sont réalisés.
- Il n'existe pas encore d'interface complète d'administration du catalogue.
- Les clubs, adhésions, profils publics et fonctions de partage ne sont pas inclus.
- L'inscription réelle auprès d'un organisateur, les paiements, objectifs et résultats sportifs restent hors périmètre.

## Déploiement IONOS

1. Créer une base MySQL `monplan`.
2. Exécuter `scripts/001_schema_identity_mysql.sql`, `scripts/002_seed_roles.sql`, puis `scripts/003_catalogue_sportif_et_plan.sql`.
3. Publier l'application en Release.
4. Injecter la chaîne de connexion par variable d'environnement.
5. Configurer HTTPS et le domaine public.

## Sécurité du dépôt public

Ne jamais versionner de secrets SMTP, API, certificats privés, chaînes de connexion réelles ou données utilisateur.

## Administration manuelle du catalogue

Le back-office est accessible sous `/administration` uniquement aux comptes ayant le rôle Identity exact `Administrateur`. Il donne accès à `/administration/manifestations` et `/administration/epreuves`. Le catalogue est, à ce stade, alimenté **manuellement** : aucun import fédéral, scraping ou import de fichier n'est réalisé.

Une manifestation est créée avec sa période, son lieu et ses informations publiques, puis une ou plusieurs épreuves peuvent lui être rattachées. La date de chaque épreuve doit appartenir à la période de sa manifestation. Les formulaires contrôlent ces règles à nouveau côté serveur.

La désactivation conserve les données : une manifestation ou une épreuve inactive disparaît du catalogue public, mais une épreuve déjà ajoutée reste signalée comme indisponible dans le plan concerné. Une manifestation ne peut être supprimée que si elle ne contient aucune épreuve. Une épreuve ne peut être supprimée que si aucune participation utilisateur ne la référence ; dans les autres cas, il faut la désactiver.

Pour attribuer le rôle localement, démarrer l'application en environnement `Development`, créer et confirmer le compte, puis associer dans les tables Identity existantes l'identifiant du compte à l'identifiant du rôle `Administrateur` dans `utilisateurs_roles` (les rôles sont initialisés au démarrage). Cette opération est réservée à une base locale maîtrisée. Aucun changement de schéma ni script SQL supplémentaire n'est nécessaire pour le back-office.
