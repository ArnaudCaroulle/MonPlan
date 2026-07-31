using Microsoft.AspNetCore.Identity;

namespace MonPlan.Services;

/// <summary>
/// Initialise les rôles applicatifs attendus au démarrage afin de préparer les autorisations MonPlan.
/// </summary>
public class ServiceInitialisationRoles(IServiceProvider services, ILogger<ServiceInitialisationRoles> logger) : IHostedService
{
    /// <summary>Expose la liste unique des rôles applicatifs initialisés, notamment le rôle protégé Administrateur.</summary>
    public static readonly IReadOnlyList<string> RolesInitialises = ["Administrateur", "Utilisateur"];
    /// <summary>
    /// Crée les rôles de base si ceux-ci ne sont pas déjà présents en base de données.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in RolesInitialises)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var resultat = await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Initialisation du rôle {Role}: {Succes}", role, resultat.Succeeded);
            }
        }
    }

    /// <summary>
    /// Ne réalise aucune action spécifique à l'arrêt de l'application.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
