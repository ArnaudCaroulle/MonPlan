using Microsoft.EntityFrameworkCore;
using MonPlan.Data;
using MonPlan.Models;

namespace MonPlan.Services;

/// <summary>
/// Alimente uniquement en environnement Development un catalogue vide avec des événements fictifs clairement identifiés comme démonstration.
/// </summary>
public class ServiceInitialisationDonneesDemo(IServiceProvider services, IHostEnvironment environnement, ILogger<ServiceInitialisationDonneesDemo> logger) : IHostedService
{
    /// <summary>
    /// Insère les manifestations et épreuves Démo au démarrage si le catalogue ne contient encore aucune manifestation.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environnement.IsDevelopment()) return;
        using var portee = services.CreateScope();
        var contexte = portee.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await contexte.ManifestationsSportives.AnyAsync(cancellationToken)) return;

        var course = new ManifestationSportive { Nom = "Courses urbaines Démo", Ville = "Ville Démo", CodePostal = "00000", DateDebut = new(2027, 4, 18), DateFin = new(2027, 4, 18), Description = "Manifestation fictive créée uniquement pour tester MonPlan." };
        course.Epreuves.Add(new EpreuveSportive { Nom = "10 km Démo", Discipline = DisciplineSportive.CourseAPied, Format = "10 km", DescriptionDistance = "10 km course à pied", DateEpreuve = course.DateDebut, HeureDepart = new(9, 0) });
        course.Epreuves.Add(new EpreuveSportive { Nom = "Semi-marathon Démo", Discipline = DisciplineSportive.CourseAPied, Format = "Semi-marathon", DescriptionDistance = "21,1 km course à pied", DateEpreuve = course.DateDebut, HeureDepart = new(10, 0) });

        var trail = new ManifestationSportive { Nom = "Trail des collines Démo", Ville = "Mont-Démo", DateDebut = new(2027, 5, 9), DateFin = new(2027, 5, 9), Description = "Parcours fictif, sans lien avec un calendrier officiel." };
        trail.Epreuves.Add(new EpreuveSportive { Nom = "Trail 25 km Démo", Discipline = DisciplineSportive.Trail, Format = "Trail 25 km", DescriptionDistance = "25 km et dénivelé fictif", DateEpreuve = trail.DateDebut, HeureDepart = new(8, 30) });

        var triathlon = new ManifestationSportive { Nom = "Triathlon du lac Démo", Ville = "Lac-Démo", DateDebut = new(2027, 6, 12), DateFin = new(2027, 6, 13), Description = "Manifestation multi-épreuves entièrement fictive." };
        triathlon.Epreuves.Add(new EpreuveSportive { Nom = "Triathlon M Démo", Discipline = DisciplineSportive.Triathlon, Format = "M", DescriptionDistance = "1,5 km natation, 40 km vélo, 10 km course à pied", DateEpreuve = new(2027, 6, 12), HeureDepart = new(14, 0) });
        triathlon.Epreuves.Add(new EpreuveSportive { Nom = "Triathlon L Démo", Discipline = DisciplineSportive.Triathlon, Format = "L", DescriptionDistance = "Distances fictives de format L", DateEpreuve = new(2027, 6, 13), HeureDepart = new(8, 0) });

        contexte.ManifestationsSportives.AddRange(course, trail, triathlon);
        await contexte.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Le catalogue sportif de démonstration a été initialisé.");
    }

    /// <summary>
    /// Ne réalise aucune opération lors de l'arrêt de l'application.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
