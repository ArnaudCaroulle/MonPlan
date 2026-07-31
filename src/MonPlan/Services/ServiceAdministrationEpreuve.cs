using Microsoft.EntityFrameworkCore;
using MonPlan.Data;
using MonPlan.Models;
using MonPlan.ViewModels;

namespace MonPlan.Services;

/// <summary>
/// Administre les épreuves, contrôle leur rattachement et leur date, et préserve celles déjà utilisées dans un plan.
/// </summary>
public class ServiceAdministrationEpreuve(ApplicationDbContext contexte)
{
    /// <summary>Recherche toutes les épreuves administratives avec des critères librement combinables.</summary>
    public async Task<IReadOnlyList<EpreuveAdminViewModel>> RechercherAsync(RechercheEpreuvesAdminViewModel c, CancellationToken cancellationToken = default)
    {
        var q = contexte.EpreuvesSportives.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(c.Texte)) { var t = c.Texte.Trim(); q = q.Where(e => e.Nom.Contains(t) || e.ManifestationSportive.Nom.Contains(t) || (e.Format != null && e.Format.Contains(t))); }
        if (c.ManifestationId.HasValue) q = q.Where(e => e.ManifestationSportiveId == c.ManifestationId.Value);
        if (!string.IsNullOrWhiteSpace(c.Ville)) { var v = c.Ville.Trim(); q = q.Where(e => e.ManifestationSportive.Ville.Contains(v)); }
        if (c.Discipline.HasValue) q = q.Where(e => e.Discipline == c.Discipline.Value);
        if (c.DateDebut.HasValue) q = q.Where(e => e.DateEpreuve >= c.DateDebut.Value);
        if (c.DateFin.HasValue) q = q.Where(e => e.DateEpreuve <= c.DateFin.Value);
        if (c.EstActive.HasValue) q = q.Where(e => e.EstActive == c.EstActive.Value);
        return await q.OrderBy(e => e.DateEpreuve).ThenBy(e => e.Nom).Select(e => new EpreuveAdminViewModel { Id = e.Id, ManifestationId = e.ManifestationSportiveId,
            Manifestation = e.ManifestationSportive.Nom, Nom = e.Nom, Discipline = e.Discipline, Format = e.Format, DescriptionDistance = e.DescriptionDistance,
            Ville = e.ManifestationSportive.Ville, DateEpreuve = e.DateEpreuve, HeureDepart = e.HeureDepart, EstActive = e.EstActive,
            EstUtilisee = e.Participations.Any() }).ToListAsync(cancellationToken);
    }

    /// <summary>Liste les manifestations disponibles avec leurs dates pour guider le formulaire.</summary>
    public Task<List<ManifestationOptionAdminViewModel>> ListerManifestationsAsync(CancellationToken cancellationToken = default) => contexte.ManifestationsSportives.AsNoTracking()
        .OrderBy(m => m.DateDebut).Select(m => new ManifestationOptionAdminViewModel { Id = m.Id, Nom = m.Nom, DateDebut = m.DateDebut, DateFin = m.DateFin }).ToListAsync(cancellationToken);

    /// <summary>Charge le détail administratif d'une épreuve, active ou inactive.</summary>
    public Task<EpreuveAdminViewModel?> ObtenirAsync(int id, CancellationToken cancellationToken = default) => contexte.EpreuvesSportives.AsNoTracking().Where(e => e.Id == id)
        .Select(e => new EpreuveAdminViewModel { Id = e.Id, ManifestationId = e.ManifestationSportiveId, Manifestation = e.ManifestationSportive.Nom, Nom = e.Nom,
            Discipline = e.Discipline, Format = e.Format, DescriptionDistance = e.DescriptionDistance, Ville = e.ManifestationSportive.Ville,
            DateEpreuve = e.DateEpreuve, HeureDepart = e.HeureDepart, EstActive = e.EstActive, EstUtilisee = e.Participations.Any() }).SingleOrDefaultAsync(cancellationToken);

    /// <summary>Charge les valeurs modifiables d'une épreuve dans un ViewModel dédié.</summary>
    public Task<FormulaireEpreuveViewModel?> ObtenirFormulaireAsync(int id, CancellationToken cancellationToken = default) => contexte.EpreuvesSportives.AsNoTracking().Where(e => e.Id == id)
        .Select(e => new FormulaireEpreuveViewModel { Id = e.Id, ManifestationSportiveId = e.ManifestationSportiveId, Nom = e.Nom, Discipline = e.Discipline,
            Format = e.Format, DescriptionDistance = e.DescriptionDistance, DateEpreuve = e.DateEpreuve, HeureDepart = e.HeureDepart, EstActive = e.EstActive }).SingleOrDefaultAsync(cancellationToken);

    /// <summary>Crée une épreuve si la manifestation existe et si la date appartient à sa période.</summary>
    public async Task<int?> CreerAsync(FormulaireEpreuveViewModel modele, CancellationToken cancellationToken = default)
    { var m = await ManifestationValideAsync(modele, cancellationToken); if (m is null) return null; var e = new EpreuveSportive { ManifestationSportiveId = m.Id, Nom = modele.Nom.Trim(),
        Discipline = modele.Discipline!.Value, Format = modele.Format?.Trim(), DescriptionDistance = modele.DescriptionDistance?.Trim(), DateEpreuve = modele.DateEpreuve!.Value,
        HeureDepart = modele.HeureDepart, EstActive = modele.EstActive }; contexte.EpreuvesSportives.Add(e); await contexte.SaveChangesAsync(cancellationToken); return e.Id; }

    /// <summary>Met à jour une épreuve après revalidation serveur de la manifestation et de la date.</summary>
    public async Task<bool> ModifierAsync(int id, FormulaireEpreuveViewModel modele, CancellationToken cancellationToken = default)
    { var m = await ManifestationValideAsync(modele, cancellationToken); if (m is null) return false; var e = await contexte.EpreuvesSportives.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (e is null) return false;
      e.ManifestationSportiveId = m.Id; e.Nom = modele.Nom.Trim(); e.Discipline = modele.Discipline!.Value; e.Format = modele.Format?.Trim(); e.DescriptionDistance = modele.DescriptionDistance?.Trim();
      e.DateEpreuve = modele.DateEpreuve!.Value; e.HeureDepart = modele.HeureDepart; e.EstActive = modele.EstActive; e.DateModification = DateTime.UtcNow; await contexte.SaveChangesAsync(cancellationToken); return true; }

    /// <summary>Active ou désactive une épreuve tout en conservant les participations existantes.</summary>
    public async Task<bool> DefinirEtatAsync(int id, bool active, CancellationToken cancellationToken = default)
    { var e = await contexte.EpreuvesSportives.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (e is null) return false; e.EstActive = active; e.DateModification = DateTime.UtcNow; await contexte.SaveChangesAsync(cancellationToken); return true; }

    /// <summary>Supprime uniquement une épreuve absente de tous les plans utilisateurs.</summary>
    public async Task<ResultatSuppressionAdmin> SupprimerAsync(int id, CancellationToken cancellationToken = default)
    { var e = await contexte.EpreuvesSportives.SingleOrDefaultAsync(x => x.Id == id, cancellationToken); if (e is null) return ResultatSuppressionAdmin.Introuvable;
      if (await contexte.ParticipationsUtilisateurs.AnyAsync(p => p.EpreuveSportiveId == id, cancellationToken)) return ResultatSuppressionAdmin.Dependances;
      contexte.Remove(e); await contexte.SaveChangesAsync(cancellationToken); return ResultatSuppressionAdmin.Supprime; }

    /// <summary>Retourne la manifestation uniquement si elle existe et contient la date choisie.</summary>
    private async Task<ManifestationSportive?> ManifestationValideAsync(FormulaireEpreuveViewModel modele, CancellationToken cancellationToken)
    { if (!modele.ManifestationSportiveId.HasValue || !modele.DateEpreuve.HasValue || !modele.Discipline.HasValue || !Enum.IsDefined(modele.Discipline.Value)) return null;
      return await contexte.ManifestationsSportives.SingleOrDefaultAsync(m => m.Id == modele.ManifestationSportiveId && modele.DateEpreuve >= m.DateDebut && modele.DateEpreuve <= m.DateFin, cancellationToken); }
}
