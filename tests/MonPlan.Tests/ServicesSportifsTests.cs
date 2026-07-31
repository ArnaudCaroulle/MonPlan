using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MonPlan.Data;
using MonPlan.Models;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Tests;

public class ServicesSportifsTests
{
    [Fact]
    public void SaisonRefuseUneDateDeFinAnterieure()
    {
        var modele = new SaisonCreationViewModel { Nom = "Saison test", DateDebut = new(2027, 12, 31), DateFin = new(2027, 1, 1) };
        var resultats = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(modele, new ValidationContext(modele), resultats, true));
        Assert.Contains(resultats, r => r.MemberNames.Contains(nameof(modele.DateFin)));
    }

    [Fact]
    public async Task CatalogueRechercheParNom()
    {
        await using var contexte = CreerContexte(); await AlimenterCatalogueAsync(contexte);
        var resultat = await new ServiceCatalogueSportif(contexte).RechercherAsync(new() { Texte = "10 km Démo" });
        Assert.Single(resultat); Assert.Equal("10 km Démo", resultat[0].Nom);
    }

    [Fact]
    public async Task CatalogueRechercheParVille()
    {
        await using var contexte = CreerContexte(); await AlimenterCatalogueAsync(contexte);
        var resultat = await new ServiceCatalogueSportif(contexte).RechercherAsync(new() { Ville = "Lac-Démo" });
        Assert.Single(resultat); Assert.Equal(DisciplineSportive.Triathlon, resultat[0].Discipline);
    }

    [Fact]
    public async Task CatalogueRechercheParDiscipline()
    {
        await using var contexte = CreerContexte(); await AlimenterCatalogueAsync(contexte);
        var resultat = await new ServiceCatalogueSportif(contexte).RechercherAsync(new() { Discipline = DisciplineSportive.Triathlon });
        Assert.Single(resultat); Assert.Equal("Triathlon M Démo", resultat[0].Nom);
    }

    [Fact]
    public async Task CatalogueRechercheParIntervalleDeDates()
    {
        await using var contexte = CreerContexte(); await AlimenterCatalogueAsync(contexte);
        var resultat = await new ServiceCatalogueSportif(contexte).RechercherAsync(new() { DateDebut = new(2027, 6, 1), DateFin = new(2027, 6, 30) });
        Assert.Single(resultat); Assert.Equal("Triathlon M Démo", resultat[0].Nom);
    }

    [Fact]
    public async Task ParticipationAccepteUneSaisonCompatible()
    {
        await using var contexte = CreerContexte(); var donnees = await AlimenterCatalogueAsync(contexte); var saison = await AjouterSaisonAsync(contexte, "u1", new(2027, 1, 1), new(2027, 12, 31));
        var resultat = await new ServiceParticipationUtilisateur(contexte).AjouterAsync("u1", donnees.Course.Id, saison.Id, StatutParticipation.Envisagee);
        Assert.Equal(ResultatAjoutParticipation.Succes, resultat); Assert.Single(contexte.ParticipationsUtilisateurs);
    }

    [Fact]
    public async Task ParticipationRefuseUneSaisonIncompatible()
    {
        await using var contexte = CreerContexte(); var donnees = await AlimenterCatalogueAsync(contexte); var saison = await AjouterSaisonAsync(contexte, "u1", new(2026, 1, 1), new(2026, 12, 31));
        var resultat = await new ServiceParticipationUtilisateur(contexte).AjouterAsync("u1", donnees.Course.Id, saison.Id, StatutParticipation.Envisagee);
        Assert.Equal(ResultatAjoutParticipation.SaisonIncompatible, resultat); Assert.Empty(contexte.ParticipationsUtilisateurs);
    }

    [Fact]
    public async Task ParticipationRefuseUnDoublon()
    {
        await using var contexte = CreerContexte(); var donnees = await AlimenterCatalogueAsync(contexte); var saison = await AjouterSaisonAsync(contexte, "u1", new(2027, 1, 1), new(2027, 12, 31)); var service = new ServiceParticipationUtilisateur(contexte);
        Assert.Equal(ResultatAjoutParticipation.Succes, await service.AjouterAsync("u1", donnees.Course.Id, saison.Id, StatutParticipation.Envisagee));
        Assert.Equal(ResultatAjoutParticipation.DejaPresente, await service.AjouterAsync("u1", donnees.Course.Id, saison.Id, StatutParticipation.Inscrit)); Assert.Single(contexte.ParticipationsUtilisateurs);
    }

    [Fact]
    public async Task ParticipationRefuseLaSaisonDUnAutreUtilisateur()
    {
        await using var contexte = CreerContexte(); var donnees = await AlimenterCatalogueAsync(contexte); var saison = await AjouterSaisonAsync(contexte, "proprietaire", new(2027, 1, 1), new(2027, 12, 31));
        var resultat = await new ServiceParticipationUtilisateur(contexte).AjouterAsync("intrus", donnees.Course.Id, saison.Id, StatutParticipation.Envisagee);
        Assert.Equal(ResultatAjoutParticipation.SaisonInterdite, resultat); Assert.Empty(contexte.ParticipationsUtilisateurs);
    }

    [Fact]
    public async Task PlansDeDeuxUtilisateursRestentIsoles()
    {
        await using var contexte = CreerContexte(); var donnees = await AlimenterCatalogueAsync(contexte); var s1 = await AjouterSaisonAsync(contexte, "u1", new(2027, 1, 1), new(2027, 12, 31)); var s2 = await AjouterSaisonAsync(contexte, "u2", new(2027, 1, 1), new(2027, 12, 31)); var service = new ServiceParticipationUtilisateur(contexte);
        await service.AjouterAsync("u1", donnees.Course.Id, s1.Id, StatutParticipation.Envisagee); await service.AjouterAsync("u2", donnees.Triathlon.Id, s2.Id, StatutParticipation.Inscrit);
        var plan1 = await service.ObtenirPlanAsync("u1"); var plan2 = await service.ObtenirPlanAsync("u2");
        Assert.Equal("10 km Démo", Assert.Single(Assert.Single(plan1).Participations).Epreuve); Assert.Equal("Triathlon M Démo", Assert.Single(Assert.Single(plan2).Participations).Epreuve);
    }

    private static ApplicationDbContext CreerContexte() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<(EpreuveSportive Course, EpreuveSportive Triathlon)> AlimenterCatalogueAsync(ApplicationDbContext contexte)
    {
        var m1 = new ManifestationSportive { Nom = "Course Démo", Ville = "Ville-Démo", DateDebut = new(2027, 4, 18), DateFin = new(2027, 4, 18) };
        var course = new EpreuveSportive { Nom = "10 km Démo", Discipline = DisciplineSportive.CourseAPied, Format = "10 km", DateEpreuve = m1.DateDebut, ManifestationSportive = m1 };
        var m2 = new ManifestationSportive { Nom = "Triathlon Démo", Ville = "Lac-Démo", DateDebut = new(2027, 6, 12), DateFin = new(2027, 6, 12) };
        var triathlon = new EpreuveSportive { Nom = "Triathlon M Démo", Discipline = DisciplineSportive.Triathlon, Format = "M", DateEpreuve = m2.DateDebut, ManifestationSportive = m2 };
        contexte.EpreuvesSportives.AddRange(course, triathlon); await contexte.SaveChangesAsync(); return (course, triathlon);
    }

    private static async Task<SaisonSportive> AjouterSaisonAsync(ApplicationDbContext contexte, string utilisateur, DateOnly debut, DateOnly fin)
    {
        var saison = new SaisonSportive { ApplicationUserId = utilisateur, Nom = $"Saison {utilisateur}", DateDebut = debut, DateFin = fin };
        contexte.SaisonsSportives.Add(saison); await contexte.SaveChangesAsync(); return saison;
    }
}
