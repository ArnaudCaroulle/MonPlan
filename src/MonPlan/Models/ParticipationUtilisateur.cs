namespace MonPlan.Models;

/// <summary>
/// Matérialise l'ajout unique d'une épreuve au plan privé d'un utilisateur.
/// </summary>
public class ParticipationUtilisateur
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public int EpreuveSportiveId { get; set; }
    public int SaisonSportiveId { get; set; }
    public StatutParticipation StatutParticipation { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public EpreuveSportive EpreuveSportive { get; set; } = null!;
    public SaisonSportive SaisonSportive { get; set; } = null!;
}
