using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonPlan.Models;

namespace MonPlan.Data;

/// <summary>
/// Centralise l'accès aux données applicatives et aux tables ASP.NET Core Identity.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ManifestationSportive> ManifestationsSportives => Set<ManifestationSportive>();
    public DbSet<EpreuveSportive> EpreuvesSportives => Set<EpreuveSportive>();
    public DbSet<SaisonSportive> SaisonsSportives => Set<SaisonSportive>();
    public DbSet<ParticipationUtilisateur> ParticipationsUtilisateurs => Set<ParticipationUtilisateur>();

    /// <summary>
    /// Configure les noms de tables, les relations logiques EF et l'unicité d'une épreuve dans le plan d'un utilisateur.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().ToTable("utilisateurs");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("utilisateurs_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("utilisateurs_revendications");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("utilisateurs_connexions");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("roles_revendications");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("utilisateurs_jetons");

        builder.Entity<ManifestationSportive>().ToTable("manifestations_sportives");
        builder.Entity<EpreuveSportive>().ToTable("epreuves_sportives");
        builder.Entity<SaisonSportive>().ToTable("saisons_sportives");
        builder.Entity<ParticipationUtilisateur>().ToTable("participations_utilisateurs");

        builder.Entity<ParticipationUtilisateur>()
            .HasIndex(p => new { p.ApplicationUserId, p.EpreuveSportiveId })
            .IsUnique();
        builder.Entity<EpreuveSportive>().HasIndex(e => e.DateEpreuve);
        builder.Entity<SaisonSportive>().HasIndex(s => new { s.ApplicationUserId, s.DateDebut });
    }
}
