using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;
using StatistiquesHGG.Infrastructure.Data;

namespace StatistiquesHGG.Business.Services;

public class StatistiquesService
{
    private readonly AppDbContext _context;

    public StatistiquesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScorePerformance>> GetClassementServicesAsync(DateTime periode)
    {
        var debutMois = new DateTime(periode.Year, periode.Month, 1);
        return await _context.ScoresPerformance
            .Include(s => s.Service)
            .Include(s => s.Details)
                .ThenInclude(d => d.Indicateur)
            .Where(s => s.Periode == debutMois)
            .OrderBy(s => s.Rang)
            .ToListAsync();
    }

    public async Task RecalculerScoresAsync(DateTime periode)
    {
        var debutMois = new DateTime(periode.Year, periode.Month, 1);
        var finMois   = debutMois.AddMonths(1).AddDays(-1);

        var services = await _context.Services.Where(s => s.Actif).ToListAsync();
        var cibles   = await _context.CiblesIndicateurs
            .Include(c => c.Indicateur)
            .Where(c => c.Actif)
            .ToListAsync();

        // Supprimer scores existants pour cette période
        var existing = await _context.ScoresPerformance
            .Where(s => s.Periode == debutMois)
            .ToListAsync();
        _context.ScoresPerformance.RemoveRange(existing);
        await _context.SaveChangesAsync();

        var scores = new List<ScorePerformance>();

        foreach (var service in services)
        {
            var ciblesService = cibles.Where(c => c.ServiceId == service.Id).ToList();
            if (!ciblesService.Any()) continue;

            var scoreDetails = new List<DetailScoreIndicateur>();
            decimal totalScore = 0;
            int nbIndicateurs  = 0;

            foreach (var cible in ciblesService)
            {
                decimal valeurReelle = await GetValeurReelleAsync(
                    service.Id, cible.IndicateurId, debutMois, finMois);

                scoreDetails.Add(new DetailScoreIndicateur
                {
                    IndicateurId = cible.IndicateurId,
                    ValeurReelle = valeurReelle,
                    ValeurCible  = cible.ValeurCible,
                    Poids        = 1.0m
                });

                if (cible.ValeurCible != 0)
                {
                    totalScore += (valeurReelle / cible.ValeurCible) * 100;
                    nbIndicateurs++;
                }
            }

            decimal scoreFinal = nbIndicateurs > 0
                ? Math.Round(totalScore / nbIndicateurs, 2) : 0;

            var niveau = scoreFinal >= 90 ? NiveauPerformance.Excellent :
                         scoreFinal >= 75 ? NiveauPerformance.Bon       :
                         scoreFinal >= 60 ? NiveauPerformance.Moyen     :
                                            NiveauPerformance.AAmeliorer;

            scores.Add(new ScorePerformance
            {
                ServiceId    = service.Id,
                Periode      = debutMois,
                ValeurReelle = scoreDetails.Sum(d => d.ValeurReelle),
                ValeurCible  = scoreDetails.Sum(d => d.ValeurCible),
                Niveau       = niveau,
                DateCalcul   = DateTime.Now,
                Details      = scoreDetails
            });
        }

        // Rangs
        var sorted = scores
            .OrderByDescending(s => s.ValeurCible > 0
                ? (s.ValeurReelle / s.ValeurCible) * 100 : 0)
            .ToList();
        for (int i = 0; i < sorted.Count; i++)
            sorted[i].Rang = i + 1;

        _context.ScoresPerformance.AddRange(scores);
        await _context.SaveChangesAsync();
    }

    private async Task<decimal> GetValeurReelleAsync(
        int serviceId, int indicateurId,
        DateTime debut, DateTime fin)
    {
        return indicateurId switch
        {
            5 => await _context.SaisiesConsultation
                    .Where(c => c.ServiceId == serviceId
                             && c.DateSaisie >= debut
                             && c.DateSaisie <= fin
                             && c.Validee)
                    .SumAsync(c => (decimal)(
                        c.NouveauxHommes + c.NouvellesFemmes + c.NouveauxEnfants +
                        c.AnciensHommes  + c.AnciennesFemmes + c.AnciensEnfants)),

            6 => await _context.SaisiesHospitalisation
                    .Where(h => h.ServiceId == serviceId
                             && h.DateSaisie >= debut
                             && h.DateSaisie <= fin
                             && h.Validee)
                    .SumAsync(h => (decimal)(h.Hommes + h.Femmes + h.Enfants)),

            7 => await _context.SaisiesActeChirurgical
                    .Where(a => a.ServiceId == serviceId
                             && a.DateSaisie >= debut
                             && a.DateSaisie <= fin
                             && a.Validee)
                    .SumAsync(a => (decimal)(a.Hommes + a.Femmes + a.Enfants)),

            8 => await _context.SaisiesConsultation
                    .Where(c => c.ServiceId == serviceId
                             && c.DateSaisie >= debut
                             && c.DateSaisie <= fin
                             && c.Validee)
                    .SumAsync(c => (decimal)(
                        c.NouveauxHommes + c.NouvellesFemmes + c.NouveauxEnfants)),

            _ => 0
        };
    }
}
