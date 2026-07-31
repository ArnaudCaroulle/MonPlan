using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonPlan.Controllers;

/// <summary>Affiche le point d'entrée du back-office exclusivement réservé aux administrateurs.</summary>
[Authorize(Roles = "Administrateur")]
public class AdministrationController : Controller
{
    /// <summary>Présente les accès à la gestion des manifestations et des épreuves.</summary>
    [HttpGet("administration")]
    public IActionResult Index() => View();
}
