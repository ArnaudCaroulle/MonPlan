using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MonPlan.Models;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>
/// Protège et affiche l'espace personnel permettant à l'utilisateur de gérer son profil MonPlan.
/// </summary>
[Authorize]
public class ProfileController(UserManager<ApplicationUser> userManager, ServiceProfilUtilisateur serviceProfilUtilisateur) : Controller
{
    /// <summary>
    /// Affiche le profil de l'utilisateur connecté.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        return user is null ? Challenge() : View(serviceProfilUtilisateur.CreerModele(user));
    }

    /// <summary>
    /// Met à jour les informations de profil saisies par l'utilisateur connecté.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfilViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        var result = await serviceProfilUtilisateur.MettreAJourAsync(user, model);
        if (result.Succeeded)
        {
            ViewData["Message"] = "Profil mis à jour.";
            return View(serviceProfilUtilisateur.CreerModele(user));
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }
}
