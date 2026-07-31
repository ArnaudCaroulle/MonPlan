using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MonPlan.Models;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>
/// Permet à l'utilisateur authentifié de consulter et créer ses propres saisons sportives.
/// </summary>
[Authorize]
public class SaisonsController(UserManager<ApplicationUser> userManager, ServiceSaisonSportive serviceSaison) : Controller
{
    /// <summary>
    /// Liste uniquement les saisons appartenant à l'utilisateur courant.
    /// </summary>
    [HttpGet("mes-saisons")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var id = userManager.GetUserId(User);
        return id is null ? Challenge() : View(await serviceSaison.ListerAsync(id, cancellationToken));
    }

    /// <summary>
    /// Affiche le formulaire de création, avec un retour facultatif vers une épreuve.
    /// </summary>
    [HttpGet("mes-saisons/creer")]
    public IActionResult Creer(int? retourEpreuveId = null) => View(new SaisonCreationViewModel { RetourEpreuveId = retourEpreuveId?.ToString() });

    /// <summary>
    /// Crée une saison privée à partir des champs validés et redirige vers le parcours d'origine.
    /// </summary>
    [HttpPost("mes-saisons/creer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Creer(SaisonCreationViewModel modele, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(modele);
        var id = userManager.GetUserId(User);
        if (id is null) return Challenge();
        await serviceSaison.CreerAsync(id, modele, cancellationToken);
        TempData["Succes"] = "Ta saison a été créée.";
        return int.TryParse(modele.RetourEpreuveId, out var epreuveId)
            ? RedirectToAction("Ajouter", "Plan", new { epreuveId })
            : RedirectToAction(nameof(Index));
    }
}
