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
    }
}
