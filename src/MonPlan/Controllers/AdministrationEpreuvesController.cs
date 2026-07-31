using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>Orchestre les écrans protégés de recherche, création et gestion des épreuves sportives.</summary>
[Authorize(Roles = "Administrateur")]
[Route("administration/epreuves")]
public class AdministrationEpreuvesController(ServiceAdministrationEpreuve service) : Controller
{
    /// <summary>Affiche les épreuves selon des filtres combinables.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] RechercheEpreuvesAdminViewModel modele, CancellationToken ct) { modele.Manifestations = await service.ListerManifestationsAsync(ct); modele.Resultats = await service.RechercherAsync(modele, ct); return View(modele); }
    /// <summary>Affiche le détail d'une épreuve et les actions autorisées.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct) { var e = await service.ObtenirAsync(id, ct); return e is null ? NotFound() : View(e); }
    /// <summary>Prépare le formulaire de création, éventuellement préselectionné depuis une manifestation.</summary>
    [HttpGet("creer")]
    public async Task<IActionResult> Creer(int? manifestationId, CancellationToken ct) => View("Formulaire", new FormulaireEpreuveViewModel { ManifestationSportiveId = manifestationId, Manifestations = await service.ListerManifestationsAsync(ct) });
    /// <summary>Crée une épreuve après revalidation de son rattachement et de sa date.</summary>
    [HttpPost("creer"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Creer(FormulaireEpreuveViewModel modele, CancellationToken ct) { if (ModelState.IsValid) { var id = await service.CreerAsync(modele, ct); if (id.HasValue) { TempData["Succes"] = "L’épreuve a été créée."; return RedirectToAction(nameof(Details), new { id }); } ModelState.AddModelError(nameof(modele.DateEpreuve), "La manifestation n’existe pas ou la date de l’épreuve est en dehors de sa période."); } return View("Formulaire", await RechargerAsync(modele, ct)); }
    /// <summary>Prépare le formulaire de modification d'une épreuve.</summary>
    [HttpGet("{id:int}/modifier")]
    public async Task<IActionResult> Modifier(int id, CancellationToken ct) { var e = await service.ObtenirFormulaireAsync(id, ct); if (e is null) return NotFound(); return View("Formulaire", await RechargerAsync(e, ct)); }
    /// <summary>Met à jour une épreuve après les contrôles métier côté serveur.</summary>
    [HttpPost("{id:int}/modifier"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Modifier(int id, FormulaireEpreuveViewModel modele, CancellationToken ct) { if (id != modele.Id) return BadRequest(); if (ModelState.IsValid && await service.ModifierAsync(id, modele, ct)) { TempData["Succes"] = "L’épreuve a été mise à jour."; return RedirectToAction(nameof(Details), new { id }); } if (ModelState.IsValid) ModelState.AddModelError(nameof(modele.DateEpreuve), "La manifestation n’existe pas ou la date de l’épreuve est en dehors de sa période."); return View("Formulaire", await RechargerAsync(modele, ct)); }
    /// <summary>Active ou désactive une épreuve sans retirer celle-ci des plans existants.</summary>
    [HttpPost("{id:int}/etat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Etat(int id, bool active, CancellationToken ct) { if (!await service.DefinirEtatAsync(id, active, ct)) return NotFound(); TempData["Succes"] = active ? "L’épreuve a été réactivée." : "L’épreuve a été désactivée."; return RedirectToAction(nameof(Details), new { id }); }
    /// <summary>Supprime une épreuve inutilisée ou explique le refus si un plan la référence.</summary>
    [HttpPost("{id:int}/supprimer"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(int id, CancellationToken ct) { var resultat = await service.SupprimerAsync(id, ct); if (resultat == ResultatSuppressionAdmin.Introuvable) return NotFound(); if (resultat == ResultatSuppressionAdmin.Dependances) { TempData["Erreur"] = "Cette épreuve est déjà présente dans le plan d’un ou plusieurs utilisateurs et ne peut pas être supprimée. Tu peux la désactiver."; return RedirectToAction(nameof(Details), new { id }); } TempData["Succes"] = "L’épreuve a été supprimée."; return RedirectToAction(nameof(Index)); }
    /// <summary>Recharge les options de manifestations sans faire confiance aux valeurs envoyées par le navigateur.</summary>
    private async Task<FormulaireEpreuveViewModel> RechargerAsync(FormulaireEpreuveViewModel modele, CancellationToken ct) { modele.Manifestations = await service.ListerManifestationsAsync(ct); return modele; }
}
