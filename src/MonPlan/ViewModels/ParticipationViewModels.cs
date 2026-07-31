using System.ComponentModel.DataAnnotations;
using MonPlan.Models;

namespace MonPlan.ViewModels;

public class AjouterParticipationViewModel
{
    public int EpreuveSportiveId { get; set; }
    public string Manifestation { get; set; } = string.Empty;
    public string Epreuve { get; set; } = string.Empty;
    public DateOnly DateEpreuve { get; set; }
    [Required(ErrorMessage = "Choisis une saison."), Display(Name = "Saison")] public int? SaisonSportiveId { get; set; }
    [Required(ErrorMessage = "Choisis un statut."), Display(Name = "Statut")] public StatutParticipation? StatutParticipation { get; set; }
    public IReadOnlyList<SaisonOptionViewModel> SaisonsCompatibles { get; set; } = [];
    public bool DejaAjoutee { get; set; }
}

public class SaisonOptionViewModel
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
}

public class PlanSaisonViewModel
{
    public int SaisonId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public IReadOnlyList<ParticipationPlanViewModel> Participations { get; set; } = [];
}

public class ParticipationPlanViewModel
{
    public int Id { get; set; }
    public int EpreuveId { get; set; }
    public DateOnly DateEpreuve { get; set; }
    public string Manifestation { get; set; } = string.Empty;
    public string Epreuve { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public DisciplineSportive Discipline { get; set; }
    public string? Format { get; set; }
    public StatutParticipation Statut { get; set; }
    public bool EstDisponible { get; set; }
}

public enum ResultatAjoutParticipation
{
    Succes,
    EpreuveIntrouvable,
    SaisonInterdite,
    SaisonIncompatible,
    DejaPresente,
    StatutInvalide
}
