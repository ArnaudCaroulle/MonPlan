using System.ComponentModel.DataAnnotations;

namespace MonPlan.Models;

/// <summary>
/// Représente une course précise proposée au sein d'une manifestation sportive.
/// </summary>
public class EpreuveSportive : IValidatableObject
{
    public int Id { get; set; }
    public int ManifestationSportiveId { get; set; }
    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(200)] public string Nom { get; set; } = string.Empty;
    [Required] public DisciplineSportive Discipline { get; set; }
    [StringLength(100)] public string? Format { get; set; }
    [StringLength(1000)] public string? DescriptionDistance { get; set; }
    [Required] public DateOnly DateEpreuve { get; set; }
    public TimeOnly? HeureDepart { get; set; }
    public bool EstActive { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
    public ManifestationSportive ManifestationSportive { get; set; } = null!;
    public ICollection<ParticipationUtilisateur> Participations { get; set; } = [];

    /// <summary>
    /// Vérifie, lorsque la manifestation est chargée, que la date de l'épreuve appartient à sa période.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ManifestationSportive is not null && (DateEpreuve < ManifestationSportive.DateDebut || DateEpreuve > ManifestationSportive.DateFin))
            yield return new ValidationResult("La date de l’épreuve doit être comprise dans la période de la manifestation.", [nameof(DateEpreuve)]);
    }
}
