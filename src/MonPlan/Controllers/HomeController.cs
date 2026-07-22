using Microsoft.AspNetCore.Mvc;

namespace MonPlan.Controllers;

/// <summary>
/// Sert les pages publiques principales de MonPlan et les écrans d'erreur génériques.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Affiche la page d'accueil publique de l'application.
    /// </summary>
    public IActionResult Index() => View();

    /// <summary>
    /// Affiche une page d'erreur fonctionnelle pour l'utilisateur final.
    /// </summary>
    [Route("erreur")]
    public IActionResult Error() => View();
}
