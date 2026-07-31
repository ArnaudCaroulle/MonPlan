using System.ComponentModel.DataAnnotations;
using MonPlan.Models;

namespace MonPlan.ViewModels;

public class RechercheCatalogueViewModel
{
    [Display(Name = "Recherche")] public string? Texte { get; set; }
    [Display(Name = "Ville")] public string? Ville { get; set; }
    [Display(Name = "Discipline")] public DisciplineSportive? Discipline { get; set; }
    [Display(Name = "Du"), DataType(DataType.Date)] public DateOnly? DateDebut { get; set; }
    [Display(Name = "Au"), DataType(DataType.Date)] public DateOnly? DateFin { get; set; }
    public IReadOnlyList<EpreuveCatalogueViewModel> Resultats { get; set; } = [];
}

public class EpreuveCatalogueViewModel
{
    public int Id { get; set; }
    public string Manifestation { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public DisciplineSportive Discipline { get; set; }
    public string? Format { get; set; }
    public string? DescriptionDistance { get; set; }
    public string Ville { get; set; } = string.Empty;
    public string? CodePostal { get; set; }
    public string Pays { get; set; } = string.Empty;
    public DateOnly DateEpreuve { get; set; }
    public TimeOnly? HeureDepart { get; set; }
    public string? DescriptionManifestation { get; set; }
    public string? UrlSiteOfficiel { get; set; }
}
