using System.ComponentModel.DataAnnotations;
using MonPlan.Models;

namespace MonPlan.ViewModels;

public class RechercheManifestationsAdminViewModel
{
    public string? Texte { get; set; }
    public string? Ville { get; set; }
    [DataType(DataType.Date)] public DateOnly? Date { get; set; }
    public bool? EstActive { get; set; }
    public IReadOnlyList<ManifestationAdminViewModel> Resultats { get; set; } = [];
}

public class ManifestationAdminViewModel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string? CodePostal { get; set; }
    public string Pays { get; set; } = string.Empty;
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
    public string? Description { get; set; }
    public string? UrlSiteOfficiel { get; set; }
    public bool EstActive { get; set; }
    public int NombreEpreuves { get; set; }
    public IReadOnlyList<EpreuveAdminViewModel> Epreuves { get; set; } = [];
}

public class FormulaireManifestationViewModel : IValidatableObject
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(200), Display(Name = "Nom")] public string Nom { get; set; } = string.Empty;
    [Required(ErrorMessage = "La ville est obligatoire."), StringLength(120), Display(Name = "Ville")] public string Ville { get; set; } = string.Empty;
    [StringLength(20), Display(Name = "Code postal")] public string? CodePostal { get; set; }
    [Required(ErrorMessage = "Le pays est obligatoire."), StringLength(100), Display(Name = "Pays")] public string Pays { get; set; } = "France";
    [Required(ErrorMessage = "La date de début est obligatoire."), DataType(DataType.Date), Display(Name = "Date de début")] public DateOnly? DateDebut { get; set; }
    [Required(ErrorMessage = "La date de fin est obligatoire."), DataType(DataType.Date), Display(Name = "Date de fin")] public DateOnly? DateFin { get; set; }
    [StringLength(4000), Display(Name = "Description")] public string? Description { get; set; }
    [Url(ErrorMessage = "L’URL du site officiel n’est pas valide."), StringLength(500), Display(Name = "URL du site officiel")] public string? UrlSiteOfficiel { get; set; }
    [Display(Name = "Active")] public bool EstActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateDebut.HasValue && DateFin.HasValue && DateFin < DateDebut)
            yield return new ValidationResult("La date de fin ne peut pas précéder la date de début.", [nameof(DateFin)]);
    }
}

public class RechercheEpreuvesAdminViewModel
{
    public string? Texte { get; set; }
    public int? ManifestationId { get; set; }
    public string? Ville { get; set; }
    public DisciplineSportive? Discipline { get; set; }
    [DataType(DataType.Date)] public DateOnly? DateDebut { get; set; }
    [DataType(DataType.Date)] public DateOnly? DateFin { get; set; }
    public bool? EstActive { get; set; }
    public IReadOnlyList<EpreuveAdminViewModel> Resultats { get; set; } = [];
    public IReadOnlyList<ManifestationOptionAdminViewModel> Manifestations { get; set; } = [];
}

public class EpreuveAdminViewModel
{
    public int Id { get; set; }
    public int ManifestationId { get; set; }
    public string Manifestation { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public DisciplineSportive Discipline { get; set; }
    public string? Format { get; set; }
    public string? DescriptionDistance { get; set; }
    public string Ville { get; set; } = string.Empty;
    public DateOnly DateEpreuve { get; set; }
    public TimeOnly? HeureDepart { get; set; }
    public bool EstActive { get; set; }
    public bool EstUtilisee { get; set; }
}

public class FormulaireEpreuveViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "La manifestation est obligatoire."), Display(Name = "Manifestation")] public int? ManifestationSportiveId { get; set; }
    [Required(ErrorMessage = "Le nom est obligatoire."), StringLength(200), Display(Name = "Nom")] public string Nom { get; set; } = string.Empty;
    [Required(ErrorMessage = "La discipline est obligatoire."), Display(Name = "Discipline")] public DisciplineSportive? Discipline { get; set; }
    [StringLength(100), Display(Name = "Format")] public string? Format { get; set; }
    [StringLength(1000), Display(Name = "Description de la distance")] public string? DescriptionDistance { get; set; }
    [Required(ErrorMessage = "La date de l’épreuve est obligatoire."), DataType(DataType.Date), Display(Name = "Date de l’épreuve")] public DateOnly? DateEpreuve { get; set; }
    [DataType(DataType.Time), Display(Name = "Heure de départ")] public TimeOnly? HeureDepart { get; set; }
    [Display(Name = "Active")] public bool EstActive { get; set; } = true;
    public IReadOnlyList<ManifestationOptionAdminViewModel> Manifestations { get; set; } = [];
}

public class ManifestationOptionAdminViewModel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
}

public enum ResultatSuppressionAdmin { Supprime, Introuvable, Dependances }
