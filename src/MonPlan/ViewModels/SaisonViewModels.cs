using System.ComponentModel.DataAnnotations;

namespace MonPlan.ViewModels;

public class SaisonCreationViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(200), Display(Name = "Nom")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de début est obligatoire."), DataType(DataType.Date), Display(Name = "Date de début")]
    public DateOnly DateDebut { get; set; }

    [Required(ErrorMessage = "La date de fin est obligatoire."), DataType(DataType.Date), Display(Name = "Date de fin")]
    public DateOnly DateFin { get; set; }

    public string? RetourEpreuveId { get; set; }

    /// <summary>
    /// Vérifie la cohérence chronologique des dates saisies.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateFin < DateDebut)
            yield return new ValidationResult("La date de fin doit être postérieure ou égale à la date de début.", [nameof(DateFin)]);
    }
}

public class SaisonListeViewModel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
    public int NombreEpreuves { get; set; }
}
