using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>Orchestre les écrans protégés de recherche, création et gestion des manifestations.</summary>
[Authorize(Roles = "Administrateur")]
[Route("administration/manifestations")]
public class AdministrationManifestationsController(ServiceAdministrationManifestation service) : Controller
{
    /// <summary>Affiche la liste filtrable de toutes les manifestations.</summary>
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] RechercheManifestationsAdminViewModel modele, CancellationToken ct) { modele.Resultats = await service.RechercherAsync(modele, ct); return View(modele); }

    /// <summary>Affiche une manifestation et toutes ses épreuves.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct) { var m = await service.ObtenirAsync(id, ct); return m is null ? NotFound() : View(m); }

    /// <summary>Prépare le formulaire de création avec la France comme pays initial.</summary>
    [HttpGet("creer")]
    public IActionResult Creer() => View("Formulaire", new FormulaireManifestationViewModel());

    /// <summary>Crée une manifestation à partir du ViewModel validé.</summary>
    [HttpPost("creer"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Creer(FormulaireManifestationViewModel modele, CancellationToken ct)
    { if (!ModelState.IsValid) return View("Formulaire", modele); var id = await service.CreerAsync(modele, ct); if (!id.HasValue) { ModelState.AddModelError(nameof(modele.DateFin), "La période de la manifestation n’est pas valide."); return View("Formulaire", modele); } TempData["Succes"] = "La manifestation a été créée."; return RedirectToAction(nameof(Details), new { id }); }

    /// <summary>Prépare le formulaire de modification d'une manifestation existante.</summary>
    [HttpGet("{id:int}/modifier")]
    public async Task<IActionResult> Modifier(int id, CancellationToken ct) { var m = await service.ObtenirFormulaireAsync(id, ct); return m is null ? NotFound() : View("Formulaire", m); }

    /// <summary>Met à jour une manifestation après validation côté serveur.</summary>
    [HttpPost("{id:int}/modifier"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Modifier(int id, FormulaireManifestationViewModel modele, CancellationToken ct)
    { if (id != modele.Id) return BadRequest(); if (!ModelState.IsValid) return View("Formulaire", modele); if (!await service.ModifierAsync(id, modele, ct)) { ModelState.AddModelError(string.Empty, "La manifestation ou sa période n’est pas valide."); return View("Formulaire", modele); } TempData["Succes"] = "La manifestation a été mise à jour."; return RedirectToAction(nameof(Details), new { id }); }

    /// <summary>Active ou désactive explicitement une manifestation sans perte de données.</summary>
    [HttpPost("{id:int}/etat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Etat(int id, bool active, CancellationToken ct) { if (!await service.DefinirEtatAsync(id, active, ct)) return NotFound(); TempData["Succes"] = active ? "La manifestation a été réactivée." : "La manifestation a été désactivée."; return RedirectToAction(nameof(Details), new { id }); }

    /// <summary>Supprime une manifestation vide ou explique le refus lorsqu'elle contient des épreuves.</summary>
    [HttpPost("{id:int}/supprimer"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(int id, CancellationToken ct) { var resultat = await service.SupprimerAsync(id, ct); if (resultat == ResultatSuppressionAdmin.Introuvable) return NotFound(); if (resultat == ResultatSuppressionAdmin.Dependances) { TempData["Erreur"] = "Cette manifestation contient des épreuves et ne peut pas être supprimée. Tu peux la désactiver."; return RedirectToAction(nameof(Details), new { id }); } TempData["Succes"] = "La manifestation a été supprimée."; return RedirectToAction(nameof(Index)); }
}
