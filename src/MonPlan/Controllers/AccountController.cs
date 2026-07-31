using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MonPlan.Models;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Controllers;

/// <summary>
/// Gère l'inscription, la connexion, la confirmation email et la récupération du mot de passe des utilisateurs MonPlan.
/// </summary>
public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ServiceEmail serviceEmail) : Controller
{
    /// <summary>
    /// Affiche le formulaire d'inscription.
    /// </summary>
    [HttpGet("inscription")]
    public IActionResult Register() => View();

    /// <summary>
    /// Crée un compte utilisateur et génère le lien de confirmation d'adresse email.
    /// </summary>
    [HttpPost("inscription")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(InscriptionViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.MotDePasse);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Utilisateur");
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token }, Request.Scheme) ?? string.Empty;
            await serviceEmail.EnvoyerConfirmationEmailAsync(user.Email!, link);
            return RedirectToAction(nameof(RegisterConfirmation));
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    /// <summary>
    /// Informe l'utilisateur qu'un email de confirmation a été préparé.
    /// </summary>
    [HttpGet("confirmation-inscription")]
    public IActionResult RegisterConfirmation() => View();

    /// <summary>
    /// Valide l'adresse email d'un compte à partir du jeton sécurisé Identity.
    /// </summary>
    [HttpGet("confirmation-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();
        var result = await userManager.ConfirmEmailAsync(user, token);
        return View(result.Succeeded);
    }

    /// <summary>
    /// Affiche le formulaire de connexion.
    /// </summary>
    [HttpGet("connexion")]
    public IActionResult Login(string? returnUrl = null) => View(new ConnexionViewModel { ReturnUrl = returnUrl });

    /// <summary>
    /// Connecte l'utilisateur si l'email est confirmé et si les identifiants sont valides.
    /// </summary>
    [HttpPost("connexion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(ConnexionViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await signInManager.PasswordSignInAsync(model.Email, model.MotDePasse, model.SeSouvenirDeMoi, lockoutOnFailure: true);
        if (result.Succeeded)
            return !string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
                ? LocalRedirect(model.ReturnUrl)
                : RedirectToAction("Index", "Profile");
        ModelState.AddModelError(string.Empty, "Connexion impossible. Vérifie ton email confirmé et ton mot de passe.");
        return View(model);
    }

    /// <summary>
    /// Déconnecte l'utilisateur courant.
    /// </summary>
    [HttpPost("deconnexion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Affiche le formulaire de demande de réinitialisation du mot de passe.
    /// </summary>
    [HttpGet("mot-de-passe-oublie")]
    public IActionResult ForgotPassword() => View();

    /// <summary>
    /// Génère un lien de réinitialisation du mot de passe sans révéler si l'email existe.
    /// </summary>
    [HttpPost("mot-de-passe-oublie")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(MotDePasseOublieViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action(nameof(ResetPassword), "Account", new { email = model.Email, code = token }, Request.Scheme) ?? string.Empty;
            await serviceEmail.EnvoyerReinitialisationMotDePasseAsync(model.Email, link);
        }
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    /// <summary>
    /// Confirme la prise en compte de la demande de réinitialisation du mot de passe.
    /// </summary>
    [HttpGet("mot-de-passe-oublie-confirmation")]
    public IActionResult ForgotPasswordConfirmation() => View();

    /// <summary>
    /// Affiche le formulaire permettant de choisir un nouveau mot de passe.
    /// </summary>
    [HttpGet("reinitialiser-mot-de-passe")]
    public IActionResult ResetPassword(string email, string code) => View(new ReinitialiserMotDePasseViewModel { Email = email, Code = code });

    /// <summary>
    /// Réinitialise le mot de passe avec le jeton Identity reçu par email.
    /// </summary>
    [HttpPost("reinitialiser-mot-de-passe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ReinitialiserMotDePasseViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null) return RedirectToAction(nameof(ResetPasswordConfirmation));
        var result = await userManager.ResetPasswordAsync(user, model.Code, model.MotDePasse);
        if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation));
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }

    /// <summary>
    /// Confirme que le mot de passe a été réinitialisé.
    /// </summary>
    [HttpGet("mot-de-passe-reinitialise")]
    public IActionResult ResetPasswordConfirmation() => View();

    /// <summary>
    /// Affiche la page d'accès refusé.
    /// </summary>
    [HttpGet("acces-refuse")]
    public IActionResult AccessDenied() => View();
}
