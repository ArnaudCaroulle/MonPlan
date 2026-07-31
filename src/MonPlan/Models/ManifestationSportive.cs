using System.ComponentModel.DataAnnotations;

namespace MonPlan.Models;

/// <summary>
/// Représente une manifestation globale qui regroupe une ou plusieurs épreuves sportives.
/// </summary>
public class ManifestationSportive : IValidatableObject
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(200)] public string Nom { get; set; } = string.Empty;
    [Required(ErrorMessage = "La ville est obligatoire."), StringLength(120)] public string Ville { get; set; } = string.Empty;
    [StringLength(20)] public string? CodePostal { get; set; }
    [Required, StringLength(100)] public string Pays { get; set; } = "France";
    [Required] public DateOnly DateDebut { get; set; }
    [Required] public DateOnly DateFin { get; set; }
    [StringLength(4000)] public string? Description { get; set; }
    [Url, StringLength(500)] public string? UrlSiteOfficiel { get; set; }
    public bool EstActive { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
    public ICollection<EpreuveSportive> Epreuves { get; set; } = [];

    /// <summary>
    /// Vérifie que la période de la manifestation est chronologique.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateFin < DateDebut)
            yield return new ValidationResult("La date de fin doit être postérieure ou égale à la date de début.", [nameof(DateFin)]);
    }
}
