using Microsoft.EntityFrameworkCore;
using MonPlan.Data;
using MonPlan.Models;
using MonPlan.ViewModels;

namespace MonPlan.Services;

/// <summary>
/// Sécurise l'ajout et la consultation des participations en contrôlant propriétaire, période compatible et absence de doublon.
/// </summary>
public class ServiceParticipationUtilisateur(ApplicationDbContext contexte)
{
    /// <summary>
    /// Ajoute une épreuve au plan après toutes les vérifications métier et retourne un résultat explicite sans faire confiance au navigateur.
    /// </summary>
    public async Task<ResultatAjoutParticipation> AjouterAsync(string utilisateurId, int epreuveId, int saisonId, StatutParticipation statut, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(statut)) return ResultatAjoutParticipation.StatutInvalide;
        var epreuve = await contexte.EpreuvesSportives.AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == epreuveId && e.EstActive && e.ManifestationSportive.EstActive, cancellationToken);
        if (epreuve is null) return ResultatAjoutParticipation.EpreuveIntrouvable;

        var saison = await contexte.SaisonsSportives.AsNoTracking().SingleOrDefaultAsync(s => s.Id == saisonId, cancellationToken);
        if (saison is null || saison.ApplicationUserId != utilisateurId) return ResultatAjoutParticipation.SaisonInterdite;
        if (epreuve.DateEpreuve < saison.DateDebut || epreuve.DateEpreuve > saison.DateFin) return ResultatAjoutParticipation.SaisonIncompatible;
        if (await contexte.ParticipationsUtilisateurs.AnyAsync(p => p.ApplicationUserId == utilisateurId && p.EpreuveSportiveId == epreuveId, cancellationToken))
            return ResultatAjoutParticipation.DejaPresente;

        contexte.ParticipationsUtilisateurs.Add(new ParticipationUtilisateur
        {
            ApplicationUserId = utilisateurId, EpreuveSportiveId = epreuveId, SaisonSportiveId = saisonId,
            StatutParticipation = statut, DateCreation = DateTime.UtcNow
        });
        try { await contexte.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            contexte.ChangeTracker.Clear();
            if (await contexte.ParticipationsUtilisateurs.AnyAsync(p => p.ApplicationUserId == utilisateurId && p.EpreuveSportiveId == epreuveId, cancellationToken))
                return ResultatAjoutParticipation.DejaPresente;
            throw;
        }
        return ResultatAjoutParticipation.Succes;
    }

    /// <summary>
    /// Construit le plan du seul utilisateur demandé, regroupé par saison et ordonné par date d'épreuve.
    /// </summary>
    public async Task<IReadOnlyList<PlanSaisonViewModel>> ObtenirPlanAsync(string utilisateurId, CancellationToken cancellationToken = default)
    {
        var elements = await contexte.ParticipationsUtilisateurs.AsNoTracking()
            .Where(p => p.ApplicationUserId == utilisateurId)
            .OrderByDescending(p => p.SaisonSportive.DateDebut).ThenBy(p => p.EpreuveSportive.DateEpreuve)
            .Select(p => new { p.SaisonSportiveId, Saison = p.SaisonSportive.Nom, p.Id, EpreuveId = p.EpreuveSportiveId,
                p.EpreuveSportive.DateEpreuve, Manifestation = p.EpreuveSportive.ManifestationSportive.Nom,
                Epreuve = p.EpreuveSportive.Nom, Ville = p.EpreuveSportive.ManifestationSportive.Ville,
                p.EpreuveSportive.Discipline, p.EpreuveSportive.Format, Statut = p.StatutParticipation })
            .ToListAsync(cancellationToken);
        return elements.GroupBy(p => new { p.SaisonSportiveId, p.Saison }).Select(g => new PlanSaisonViewModel
        {
            SaisonId = g.Key.SaisonSportiveId, Nom = g.Key.Saison,
            Participations = g.Select(p => new ParticipationPlanViewModel { Id = p.Id, EpreuveId = p.EpreuveId, DateEpreuve = p.DateEpreuve,
                Manifestation = p.Manifestation, Epreuve = p.Epreuve, Ville = p.Ville, Discipline = p.Discipline, Format = p.Format, Statut = p.Statut }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Indique si l'épreuve figure déjà dans le plan privé de l'utilisateur.
    /// </summary>
    public Task<bool> ExisteAsync(string utilisateurId, int epreuveId, CancellationToken cancellationToken = default) =>
        contexte.ParticipationsUtilisateurs.AnyAsync(p => p.ApplicationUserId == utilisateurId && p.EpreuveSportiveId == epreuveId, cancellationToken);
}
