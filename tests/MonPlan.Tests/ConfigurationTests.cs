using MonPlan.Models;
using MonPlan.Services;
using Xunit;

namespace MonPlan.Tests;

public class ConfigurationTests
{
    [Fact]
    public void Utilisateur_Definit_Fuseau_Horaire_Paris_Par_Defaut()
    {
        var utilisateur = new ApplicationUser();
        Assert.Equal("Europe/Paris", utilisateur.FuseauHoraire);
    }

    [Fact]
    public void Services_Respectent_Le_Nommage_ServiceXxxxx()
    {
        Assert.StartsWith("Service", nameof(ServiceEmail));
        Assert.StartsWith("Service", nameof(ServiceProfilUtilisateur));
        Assert.StartsWith("Service", nameof(ServiceCatalogueSportif));
        Assert.StartsWith("Service", nameof(ServiceSaisonSportive));
        Assert.StartsWith("Service", nameof(ServiceParticipationUtilisateur));
        Assert.StartsWith("Service", nameof(ServiceInitialisationDonneesDemo));
    }
}
