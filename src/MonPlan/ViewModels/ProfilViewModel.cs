using System.ComponentModel.DataAnnotations;

namespace MonPlan.ViewModels;

public class ProfilViewModel
{
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [StringLength(80)] public string? Prenom { get; set; }
    [StringLength(80)] public string? Nom { get; set; }
    [Required] public string FuseauHoraire { get; set; } = "Europe/Paris";
}
