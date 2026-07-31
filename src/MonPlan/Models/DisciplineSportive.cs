using System.ComponentModel.DataAnnotations;

namespace MonPlan.Models;

/// <summary>
/// Définit les disciplines disponibles dans le catalogue sportif interne.
/// </summary>
public enum DisciplineSportive
{
    [Display(Name = "Course à pied")] CourseAPied = 1,
    [Display(Name = "Trail")] Trail,
    [Display(Name = "Cross")] Cross,
    [Display(Name = "Triathlon")] Triathlon,
    [Display(Name = "Duathlon")] Duathlon,
    [Display(Name = "Aquathlon")] Aquathlon,
    [Display(Name = "Swimrun")] Swimrun,
    [Display(Name = "Autre")] Autre
}

/// <summary>
/// Définit les deux états possibles d'une épreuve ajoutée au plan.
/// </summary>
public enum StatutParticipation
{
    [Display(Name = "Envisagée")] Envisagee = 1,
    [Display(Name = "Inscrit")] Inscrit
}
