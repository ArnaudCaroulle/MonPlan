using Microsoft.EntityFrameworkCore;
using MonPlan.Data;
using MonPlan.Models;
using MonPlan.ViewModels;

namespace MonPlan.Services;

/// <summary>
/// Gère la création et la lecture des saisons en isolant systématiquement les données par utilisateur connecté.
/// </summary>
public class ServiceSaisonSportive(ApplicationDbContext contexte)
{
    /// <summary>
    /// Retourne les saisons privées d'un utilisateur, des plus récentes aux plus anciennes, avec leur nombre d'épreuves.
    /// </summary>
    public async Task<IReadOnlyList<SaisonListeViewModel>> ListerAsync(string utilisateurId, CancellationToken cancellationToken = default) =>
        await contexte.SaisonsSportives.AsNoTracking().Where(s => s.ApplicationUserId == utilisateurId)
            .OrderByDescending(s => s.DateDebut)
            .Select(s => new SaisonListeViewModel { Id = s.Id, Nom = s.Nom, DateDebut = s.DateDebut, DateFin = s.DateFin, NombreEpreuves = s.Participations.Count })
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Crée une saison au nom de l'utilisateur courant après validation de sa période.
    /// </summary>
    public async Task CreerAsync(string utilisateurId, SaisonCreationViewModel modele, CancellationToken cancellationToken = default)
    {
        if (modele.DateFin < modele.DateDebut) throw new ArgumentException("La période de la saison est invalide.", nameof(modele));
        contexte.SaisonsSportives.Add(new SaisonSportive
        {
            ApplicationUserId = utilisateurId, Nom = modele.Nom.Trim(), DateDebut = modele.DateDebut,
            DateFin = modele.DateFin, DateCreation = DateTime.UtcNow
        });
        await contexte.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Retourne uniquement les saisons de l'utilisateur qui englobent la date indiquée.
    /// </summary>
    public async Task<IReadOnlyList<SaisonOptionViewModel>> ListerCompatiblesAsync(string utilisateurId, DateOnly date, CancellationToken cancellationToken = default) =>
        await contexte.SaisonsSportives.AsNoTracking()
            .Where(s => s.ApplicationUserId == utilisateurId && s.DateDebut <= date && s.DateFin >= date)
            .OrderByDescending(s => s.DateDebut)
            .Select(s => new SaisonOptionViewModel { Id = s.Id, Nom = s.Nom })
            .ToListAsync(cancellationToken);
}
