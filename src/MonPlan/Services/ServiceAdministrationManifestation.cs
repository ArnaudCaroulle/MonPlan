using Microsoft.EntityFrameworkCore;
using MonPlan.Data;
using MonPlan.Models;
using MonPlan.ViewModels;

namespace MonPlan.Services;

/// <summary>
/// Administre les manifestations du catalogue et protège leur suppression lorsque des épreuves y sont rattachées.
/// </summary>
public class ServiceAdministrationManifestation(ApplicationDbContext contexte)
{
    /// <summary>Recherche toutes les manifestations, actives ou non, à l'aide de critères combinables.</summary>
    public async Task<IReadOnlyList<ManifestationAdminViewModel>> RechercherAsync(RechercheManifestationsAdminViewModel criteres, CancellationToken cancellationToken = default)
    {
        var requete = contexte.ManifestationsSportives.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(criteres.Texte)) { var texte = criteres.Texte.Trim(); requete = requete.Where(m => m.Nom.Contains(texte)); }
        if (!string.IsNullOrWhiteSpace(criteres.Ville)) { var ville = criteres.Ville.Trim(); requete = requete.Where(m => m.Ville.Contains(ville)); }
        if (criteres.Date.HasValue) requete = requete.Where(m => m.DateDebut <= criteres.Date.Value && m.DateFin >= criteres.Date.Value);
        if (criteres.EstActive.HasValue) requete = requete.Where(m => m.EstActive == criteres.EstActive.Value);
        return await requete.OrderBy(m => m.DateDebut).ThenBy(m => m.Nom).Select(m => new ManifestationAdminViewModel
        { Id = m.Id, Nom = m.Nom, Ville = m.Ville, CodePostal = m.CodePostal, Pays = m.Pays, DateDebut = m.DateDebut, DateFin = m.DateFin,
          EstActive = m.EstActive, NombreEpreuves = m.Epreuves.Count }).ToListAsync(cancellationToken);
    }

    /// <summary>Charge le détail administratif d'une manifestation, y compris ses épreuves inactives.</summary>
    public async Task<ManifestationAdminViewModel?> ObtenirAsync(int id, CancellationToken cancellationToken = default) =>
        await contexte.ManifestationsSportives.AsNoTracking().Where(m => m.Id == id).Select(m => new ManifestationAdminViewModel
        { Id = m.Id, Nom = m.Nom, Ville = m.Ville, CodePostal = m.CodePostal, Pays = m.Pays, DateDebut = m.DateDebut, DateFin = m.DateFin,
          Description = m.Description, UrlSiteOfficiel = m.UrlSiteOfficiel, EstActive = m.EstActive, NombreEpreuves = m.Epreuves.Count,
          Epreuves = m.Epreuves.OrderBy(e => e.DateEpreuve).Select(e => new EpreuveAdminViewModel { Id = e.Id, ManifestationId = m.Id, Manifestation = m.Nom,
              Nom = e.Nom, Discipline = e.Discipline, Format = e.Format, DateEpreuve = e.DateEpreuve, EstActive = e.EstActive }).ToList() }).SingleOrDefaultAsync(cancellationToken);

    /// <summary>Charge les valeurs modifiables d'une manifestation sans exposer directement l'entité EF.</summary>
    public Task<FormulaireManifestationViewModel?> ObtenirFormulaireAsync(int id, CancellationToken cancellationToken = default) =>
        contexte.ManifestationsSportives.AsNoTracking().Where(m => m.Id == id).Select(m => new FormulaireManifestationViewModel { Id = m.Id, Nom = m.Nom,
            Ville = m.Ville, CodePostal = m.CodePostal, Pays = m.Pays, DateDebut = m.DateDebut, DateFin = m.DateFin, Description = m.Description,
            UrlSiteOfficiel = m.UrlSiteOfficiel, EstActive = m.EstActive }).SingleOrDefaultAsync(cancellationToken);

    /// <summary>Crée une manifestation après validation de sa période.</summary>
    public async Task<int?> CreerAsync(FormulaireManifestationViewModel modele, CancellationToken cancellationToken = default)
    {
        if (!DatesValides(modele)) return null;
        var entite = new ManifestationSportive { Nom = modele.Nom.Trim(), Ville = modele.Ville.Trim(), CodePostal = modele.CodePostal?.Trim(), Pays = modele.Pays.Trim(),
            DateDebut = modele.DateDebut!.Value, DateFin = modele.DateFin!.Value, Description = modele.Description?.Trim(), UrlSiteOfficiel = modele.UrlSiteOfficiel?.Trim(), EstActive = modele.EstActive };
        contexte.ManifestationsSportives.Add(entite); await contexte.SaveChangesAsync(cancellationToken); return entite.Id;
    }

    /// <summary>Met à jour une manifestation existante après validation de sa période.</summary>
    public async Task<bool> ModifierAsync(int id, FormulaireManifestationViewModel modele, CancellationToken cancellationToken = default)
    {
        if (!DatesValides(modele)) return false;
        var entite = await contexte.ManifestationsSportives.SingleOrDefaultAsync(m => m.Id == id, cancellationToken); if (entite is null) return false;
        entite.Nom = modele.Nom.Trim(); entite.Ville = modele.Ville.Trim(); entite.CodePostal = modele.CodePostal?.Trim(); entite.Pays = modele.Pays.Trim();
        entite.DateDebut = modele.DateDebut!.Value; entite.DateFin = modele.DateFin!.Value; entite.Description = modele.Description?.Trim();
        entite.UrlSiteOfficiel = modele.UrlSiteOfficiel?.Trim(); entite.EstActive = modele.EstActive; entite.DateModification = DateTime.UtcNow;
        await contexte.SaveChangesAsync(cancellationToken); return true;
    }

    /// <summary>Active ou désactive la manifestation sans supprimer ses données.</summary>
    public async Task<bool> DefinirEtatAsync(int id, bool active, CancellationToken cancellationToken = default)
    { var entite = await contexte.ManifestationsSportives.SingleOrDefaultAsync(m => m.Id == id, cancellationToken); if (entite is null) return false; entite.EstActive = active; entite.DateModification = DateTime.UtcNow; await contexte.SaveChangesAsync(cancellationToken); return true; }

    /// <summary>Supprime uniquement une manifestation ne contenant aucune épreuve.</summary>
    public async Task<ResultatSuppressionAdmin> SupprimerAsync(int id, CancellationToken cancellationToken = default)
    { var entite = await contexte.ManifestationsSportives.Include(m => m.Epreuves).SingleOrDefaultAsync(m => m.Id == id, cancellationToken); if (entite is null) return ResultatSuppressionAdmin.Introuvable; if (entite.Epreuves.Count != 0) return ResultatSuppressionAdmin.Dependances; contexte.Remove(entite); await contexte.SaveChangesAsync(cancellationToken); return ResultatSuppressionAdmin.Supprime; }

    /// <summary>Vérifie que les deux bornes existent et forment une période chronologique.</summary>
    private static bool DatesValides(FormulaireManifestationViewModel modele) => modele.DateDebut.HasValue && modele.DateFin.HasValue && modele.DateFin >= modele.DateDebut;
}
