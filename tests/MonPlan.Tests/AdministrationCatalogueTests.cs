using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MonPlan.Controllers;
using MonPlan.Data;
using MonPlan.Models;
using MonPlan.Services;
using MonPlan.ViewModels;

namespace MonPlan.Tests;

public class AdministrationCatalogueTests
{
    [Fact]
    public void ControleursAdministrationExigentLeRoleAdministrateur()
    {
        foreach (var type in new[] { typeof(AdministrationController), typeof(AdministrationManifestationsController), typeof(AdministrationEpreuvesController) })
            Assert.Equal("Administrateur", Assert.Single(type.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Roles);
    }

    [Fact]
    public void AdministrateurCorrespondAuRoleInitialise() => Assert.Contains("Administrateur", ServiceInitialisationRoles.RolesInitialises);

    [Fact]
    public async Task CreeUneManifestationValide()
    { await using var c = Contexte(); var id = await new ServiceAdministrationManifestation(c).CreerAsync(Manifestation()); Assert.NotNull(id); Assert.Equal("France", (await c.ManifestationsSportives.FindAsync(id))!.Pays); }

    [Fact]
    public async Task RefuseUnePeriodeDeManifestationInversee()
    { await using var c = Contexte(); var m = Manifestation(); m.DateFin = m.DateDebut!.Value.AddDays(-1); Assert.Null(await new ServiceAdministrationManifestation(c).CreerAsync(m)); }

    [Fact]
    public async Task CreeUneEpreuveRattachee()
    { await using var c = Contexte(); var m = await AjouterManifestation(c); var id = await new ServiceAdministrationEpreuve(c).CreerAsync(Epreuve(m.Id)); Assert.NotNull(id); Assert.Equal(m.Id, (await c.EpreuvesSportives.FindAsync(id))!.ManifestationSportiveId); }

    [Fact]
    public async Task RefuseUneEpreuveHorsPeriode()
    { await using var c = Contexte(); var m = await AjouterManifestation(c); var e = Epreuve(m.Id); e.DateEpreuve = m.DateFin.AddDays(1); Assert.Null(await new ServiceAdministrationEpreuve(c).CreerAsync(e)); }

    [Fact]
    public async Task DesactiveUneManifestation()
    { await using var c = Contexte(); var m = await AjouterManifestation(c); Assert.True(await new ServiceAdministrationManifestation(c).DefinirEtatAsync(m.Id, false)); Assert.False(m.EstActive); }

    [Fact]
    public async Task DesactiveUneEpreuve()
    { await using var c = Contexte(); var (m, e) = await AjouterEpreuve(c); Assert.True(await new ServiceAdministrationEpreuve(c).DefinirEtatAsync(e.Id, false)); Assert.False(e.EstActive); }

    [Fact]
    public async Task RefuseDeSupprimerUneManifestationAvecEpreuve()
    { await using var c = Contexte(); var (m, _) = await AjouterEpreuve(c); Assert.Equal(ResultatSuppressionAdmin.Dependances, await new ServiceAdministrationManifestation(c).SupprimerAsync(m.Id)); }

    [Fact]
    public async Task RefuseDeSupprimerUneEpreuveUtilisee()
    { await using var c = Contexte(); var (_, e) = await AjouterEpreuve(c); c.ParticipationsUtilisateurs.Add(new() { ApplicationUserId="u", EpreuveSportiveId=e.Id, SaisonSportiveId=1 }); await c.SaveChangesAsync(); Assert.Equal(ResultatSuppressionAdmin.Dependances, await new ServiceAdministrationEpreuve(c).SupprimerAsync(e.Id)); }

    [Fact]
    public async Task SupprimeUneEpreuveInutilisee()
    { await using var c = Contexte(); var (_, e) = await AjouterEpreuve(c); Assert.Equal(ResultatSuppressionAdmin.Supprime, await new ServiceAdministrationEpreuve(c).SupprimerAsync(e.Id)); Assert.Null(await c.EpreuvesSportives.FindAsync(e.Id)); }

    [Fact]
    public async Task EpreuveInactiveAbsenteDuCatalogue()
    { await using var c = Contexte(); var (_, e) = await AjouterEpreuve(c); await new ServiceAdministrationEpreuve(c).DefinirEtatAsync(e.Id, false); Assert.Empty(await new ServiceCatalogueSportif(c).RechercherAsync(new())); }

    [Fact]
    public async Task EpreuveInactiveResteDansLePlanCommeIndisponible()
    { await using var c = Contexte(); var (_, e) = await AjouterEpreuve(c); var saison = new SaisonSportive { Id=1, ApplicationUserId="u", Nom="2027", DateDebut=new(2027,1,1), DateFin=new(2027,12,31) }; c.SaisonsSportives.Add(saison); c.ParticipationsUtilisateurs.Add(new() { ApplicationUserId="u", EpreuveSportiveId=e.Id, SaisonSportive=saison }); await c.SaveChangesAsync(); await new ServiceAdministrationEpreuve(c).DefinirEtatAsync(e.Id, false); var item = Assert.Single(Assert.Single(await new ServiceParticipationUtilisateur(c).ObtenirPlanAsync("u")).Participations); Assert.False(item.EstDisponible); }

    private static ApplicationDbContext Contexte() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static FormulaireManifestationViewModel Manifestation() => new() { Nom="Triathlon Démo", Ville="Gravelines", Pays="France", DateDebut=new(2027,6,1), DateFin=new(2027,6,2) };
    private static FormulaireEpreuveViewModel Epreuve(int manifestationId) => new() { ManifestationSportiveId=manifestationId, Nom="Triathlon S", Discipline=DisciplineSportive.Triathlon, DateEpreuve=new(2027,6,1) };
    private static async Task<ManifestationSportive> AjouterManifestation(ApplicationDbContext c) { var m=new ManifestationSportive { Nom="Triathlon", Ville="Gravelines", Pays="France", DateDebut=new(2027,6,1), DateFin=new(2027,6,2) }; c.Add(m); await c.SaveChangesAsync(); return m; }
    private static async Task<(ManifestationSportive, EpreuveSportive)> AjouterEpreuve(ApplicationDbContext c) { var m=await AjouterManifestation(c); var e=new EpreuveSportive { ManifestationSportiveId=m.Id, ManifestationSportive=m, Nom="Triathlon S", Discipline=DisciplineSportive.Triathlon, DateEpreuve=m.DateDebut }; c.Add(e); await c.SaveChangesAsync(); return (m,e); }
}
