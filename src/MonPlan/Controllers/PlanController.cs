using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MonPlan.Models;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>
/// Gère le plan privé de l'utilisateur et orchestre l'ajout sécurisé d'une épreuve à une saison compatible.
/// </summary>
[Authorize]
public class PlanController(UserManager<ApplicationUser> userManager, ServiceCatalogueSportif serviceCatalogue, ServiceSaisonSportive serviceSaison, ServiceParticipationUtilisateur serviceParticipation) : Controller
{
    /// <summary>
    /// Affiche les participations du seul utilisateur connecté, regroupées par saison.
    /// </summary>
    [HttpGet("mon-plan")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var id = userManager.GetUserId(User);
        return id is null ? Challenge() : View(await serviceParticipation.ObtenirPlanAsync(id, cancellationToken));
    }

    /// <summary>
    /// Prépare le formulaire avec l'épreuve et les seules saisons compatibles de l'utilisateur.
    /// </summary>
    [HttpGet("mon-plan/ajouter/{epreuveId:int}")]
    public async Task<IActionResult> Ajouter(int epreuveId, CancellationToken cancellationToken)
    {
        var id = userManager.GetUserId(User);
        if (id is null) return Challenge();
        var modele = await ConstruireModeleAsync(id, epreuveId, cancellationToken);
        return modele is null ? NotFound() : View(modele);
    }

    /// <summary>
    /// Enregistre la participation après revalidation serveur du propriétaire, de la période et de l'unicité.
    /// </summary>
    [HttpPost("mon-plan/ajouter/{epreuveId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajouter(int epreuveId, AjouterParticipationViewModel modele, CancellationToken cancellationToken)
    {
        var id = userManager.GetUserId(User);
        if (id is null) return Challenge();
        if (epreuveId != modele.EpreuveSportiveId) return BadRequest();
        if (ModelState.IsValid)
        {
            var resultat = await serviceParticipation.AjouterAsync(id, epreuveId, modele.SaisonSportiveId!.Value, modele.StatutParticipation!.Value, cancellationToken);
            if (resultat == ResultatAjoutParticipation.Succes)
            {
                TempData["Succes"] = "L’épreuve a été ajoutée à ton plan.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, resultat switch
            {
                ResultatAjoutParticipation.DejaPresente => "Cette épreuve est déjà présente dans ton plan.",
                ResultatAjoutParticipation.SaisonIncompatible => "La saison choisie ne contient pas la date de cette épreuve.",
                ResultatAjoutParticipation.SaisonInterdite => "La saison choisie ne t’appartient pas.",
                ResultatAjoutParticipation.StatutInvalide => "Le statut choisi n’est pas valide.",
                _ => "Cette épreuve n’est plus disponible."
            });
        }
        var recharge = await ConstruireModeleAsync(id, epreuveId, cancellationToken);
        if (recharge is null) return NotFound();
        recharge.SaisonSportiveId = modele.SaisonSportiveId;
        recharge.StatutParticipation = modele.StatutParticipation;
        return View(recharge);
    }

    /// <summary>
    /// Recharge depuis les services les informations affichées et les saisons compatibles sans reprendre de données sensibles du formulaire.
    /// </summary>
    private async Task<AjouterParticipationViewModel?> ConstruireModeleAsync(string utilisateurId, int epreuveId, CancellationToken cancellationToken)
    {
        var epreuve = await serviceCatalogue.ObtenirAsync(epreuveId, cancellationToken);
        if (epreuve is null) return null;
        return new AjouterParticipationViewModel
        {
            EpreuveSportiveId = epreuve.Id, Manifestation = epreuve.Manifestation, Epreuve = epreuve.Nom, DateEpreuve = epreuve.DateEpreuve,
            SaisonsCompatibles = await serviceSaison.ListerCompatiblesAsync(utilisateurId, epreuve.DateEpreuve, cancellationToken),
            StatutParticipation = StatutParticipation.Envisagee,
            DejaAjoutee = await serviceParticipation.ExisteAsync(utilisateurId, epreuveId, cancellationToken)
        };
    }
}
