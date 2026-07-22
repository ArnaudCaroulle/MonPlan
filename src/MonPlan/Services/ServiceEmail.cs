namespace MonPlan.Services;

/// <summary>
/// Prépare l'envoi des emails transactionnels MonPlan; en première version, les messages sont journalisés pour éviter tout secret SMTP dans le dépôt public.
/// </summary>
public class ServiceEmail(ILogger<ServiceEmail> logger, IConfiguration configuration)
{
    /// <summary>
    /// Journalise un lien de confirmation d'email généré par ASP.NET Core Identity.
    /// </summary>
    public Task EnvoyerConfirmationEmailAsync(string email, string lienConfirmation)
    {
        logger.LogInformation("Confirmation email MonPlan pour {Email}. Expéditeur configuré: {From}. Lien: {Lien}", email, configuration["Email:From"], lienConfirmation);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Journalise un lien de réinitialisation de mot de passe généré par ASP.NET Core Identity.
    /// </summary>
    public Task EnvoyerReinitialisationMotDePasseAsync(string email, string lienReinitialisation)
    {
        logger.LogInformation("Réinitialisation de mot de passe MonPlan pour {Email}. Lien: {Lien}", email, lienReinitialisation);
        return Task.CompletedTask;
    }
}
