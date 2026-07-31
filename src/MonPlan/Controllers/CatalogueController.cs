using Microsoft.AspNetCore.Mvc;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>
/// Présente publiquement le catalogue sportif interne et ses filtres côté serveur.
/// </summary>
public class CatalogueController(ServiceCatalogueSportif serviceCatalogue) : Controller
{
    /// <summary>
    /// Affiche les épreuves actives correspondant aux critères GET combinés.
    /// </summary>
    [HttpGet("catalogue")]
    public async Task<IActionResult> Index([FromQuery] RechercheCatalogueViewModel modele, CancellationToken cancellationToken)
    {
        modele.Resultats = await serviceCatalogue.RechercherAsync(modele, cancellationToken);
        return View(modele);
    }

    /// <summary>
    /// Affiche toutes les informations disponibles sur une épreuve active.
    /// </summary>
    [HttpGet("catalogue/epreuve/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var epreuve = await serviceCatalogue.ObtenirAsync(id, cancellationToken);
        return epreuve is null ? NotFound() : View(epreuve);
    }
}
