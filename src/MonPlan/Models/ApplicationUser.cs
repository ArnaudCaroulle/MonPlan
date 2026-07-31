using Microsoft.AspNetCore.Identity;

namespace MonPlan.Models;

/// <summary>
/// Représente un utilisateur MonPlan avec les informations de profil utiles à la saison sportive.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? Prenom { get; set; }
    public string? Nom { get; set; }
    public string FuseauHoraire { get; set; } = "Europe/Paris";
    public DateTime DateCreationUtc { get; set; } = DateTime.UtcNow;
}
