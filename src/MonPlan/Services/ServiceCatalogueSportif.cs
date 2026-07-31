using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MonPlan.Models;
using MonPlan.Data;
using MonPlan.ViewModels;

namespace MonPlan.Services;

/// <summary>
/// Interroge le catalogue interne et applique côté serveur les filtres combinables sur les seules épreuves actives.
/// </summary>
public class ServiceCatalogueSportif(ApplicationDbContext contexte)
{
    /// <summary>
    /// Recherche les épreuves actives par texte, ville, discipline et intervalle de dates, puis les classe chronologiquement.
    /// </summary>
    public async Task<IReadOnlyList<EpreuveCatalogueViewModel>> RechercherAsync(RechercheCatalogueViewModel criteres, CancellationToken cancellationToken = default)
    {
        var requete = contexte.EpreuvesSportives.AsNoTracking()
            .Where(e => e.EstActive && e.ManifestationSportive.EstActive);

        if (!string.IsNullOrWhiteSpace(criteres.Texte))
        {
            var texte = criteres.Texte.Trim();
            requete = requete.Where(e => e.Nom.Contains(texte) || e.ManifestationSportive.Nom.Contains(texte) || (e.Format != null && e.Format.Contains(texte)));
        }
        if (!string.IsNullOrWhiteSpace(criteres.Ville))
        {
            var ville = criteres.Ville.Trim();
            requete = requete.Where(e => e.ManifestationSportive.Ville.Contains(ville));
        }
        if (criteres.Discipline.HasValue)
            requete = requete.Where(e => e.Discipline == criteres.Discipline.Value);
        if (criteres.DateDebut.HasValue)
            requete = requete.Where(e => e.DateEpreuve >= criteres.DateDebut.Value);
        if (criteres.DateFin.HasValue)
            requete = requete.Where(e => e.DateEpreuve <= criteres.DateFin.Value);

        return await requete.OrderBy(e => e.DateEpreuve).ThenBy(e => e.Nom)
            .Select(Projection).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Charge le détail d'une épreuve active appartenant à une manifestation active.
    /// </summary>
    public Task<EpreuveCatalogueViewModel?> ObtenirAsync(int id, CancellationToken cancellationToken = default) =>
        contexte.EpreuvesSportives.AsNoTracking()
            .Where(e => e.Id == id && e.EstActive && e.ManifestationSportive.EstActive)
            .Select(Projection).SingleOrDefaultAsync(cancellationToken);

    private static readonly Expression<Func<EpreuveSportive, EpreuveCatalogueViewModel>> Projection = e => new()
    {
        Id = e.Id, Manifestation = e.ManifestationSportive.Nom, Nom = e.Nom, Discipline = e.Discipline,
        Format = e.Format, DescriptionDistance = e.DescriptionDistance, Ville = e.ManifestationSportive.Ville,
        CodePostal = e.ManifestationSportive.CodePostal, Pays = e.ManifestationSportive.Pays,
        DateEpreuve = e.DateEpreuve, HeureDepart = e.HeureDepart,
        DescriptionManifestation = e.ManifestationSportive.Description, UrlSiteOfficiel = e.ManifestationSportive.UrlSiteOfficiel
    };
}
