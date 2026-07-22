using Microsoft.AspNetCore.Identity;
using MonPlan.Models;
using MonPlan.ViewModels;

namespace MonPlan.Services;

/// <summary>
/// Gère la lecture et la mise à jour du profil utilisateur connecté sans exposer la logique Identity aux contrôleurs.
/// </summary>
public class ServiceProfilUtilisateur(UserManager<ApplicationUser> userManager)
{
    /// <summary>
    /// Construit le modèle d'affichage du profil à partir de l'utilisateur MonPlan.
    /// </summary>
    public ProfilViewModel CreerModele(ApplicationUser utilisateur) => new()
    {
        Email = utilisateur.Email ?? string.Empty,
        Prenom = utilisateur.Prenom,
        Nom = utilisateur.Nom,
        FuseauHoraire = utilisateur.FuseauHoraire
    };

    /// <summary>
    /// Applique les informations saisies au profil et persiste les changements via ASP.NET Core Identity.
    /// </summary>
    public async Task<IdentityResult> MettreAJourAsync(ApplicationUser utilisateur, ProfilViewModel modele)
    {
        utilisateur.Prenom = modele.Prenom;
        utilisateur.Nom = modele.Nom;
        utilisateur.FuseauHoraire = string.IsNullOrWhiteSpace(modele.FuseauHoraire) ? "Europe/Paris" : modele.FuseauHoraire;
        return await userManager.UpdateAsync(utilisateur);
    }
}
