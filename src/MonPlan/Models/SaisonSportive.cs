using System.ComponentModel.DataAnnotations;

namespace MonPlan.Models;

/// <summary>
/// Représente une période sportive privée appartenant à un utilisateur.
/// </summary>
public class SaisonSportive : IValidatableObject
{
    public int Id { get; set; }
    [Required] public string ApplicationUserId { get; set; } = string.Empty;
    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(200)] public string Nom { get; set; } = string.Empty;
    [Required(ErrorMessage = "La date de début est obligatoire.")] public DateOnly DateDebut { get; set; }
    [Required(ErrorMessage = "La date de fin est obligatoire.")] public DateOnly DateFin { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public ICollection<ParticipationUtilisateur> Participations { get; set; } = [];

    /// <summary>
    /// Vérifie que la saison se termine au plus tôt à sa date de début.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateFin < DateDebut)
            yield return new ValidationResult("La date de fin doit être postérieure ou égale à la date de début.", [nameof(DateFin)]);
    }
}
