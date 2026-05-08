using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using StatistiquesHGG.Core.Entities;

namespace StatistiquesHGG.UI;

public class DashboardViewModel : BaseViewModel, ILoadable
{
    private readonly AppDbContext _context;
    private readonly StatistiquesService _statsService;
    private readonly PatientRmaService _patientService;
    private bool _isLoading = true;
    private DashboardStats _stats = new();

    public DashboardViewModel(AppDbContext context, StatistiquesService statsService, PatientRmaService patientService)
    {
        _context = context;
        _statsService = statsService;
        _patientService = patientService;
        PeriodeLabel = DateTime.Now.ToString("MMMM yyyy");
        ActualiserCommand = new RelayCommand(async () => await ChargerAsync());
    }

    public bool   IsLoading    { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public string PeriodeLabel { get; set; }

    // KPIs
    public int    ConsultationsDuMois   { get => _stats.ConsultationsDuMois; }
    public int    NouveauxPatientsMois  { get => _stats.NouveauxPatientsMois; }  // v5
    public int    HospitalisationsTotal { get => _stats.HospitalisationsTotal; }
    public int    DecesDuMoisHopital    { get => _stats.DecesDuMoisHopital; }     // v5: hopital only
    public int    DecesDuMois           { get => _stats.DecesDuMois; }            // Legacy total
    public int    SaisiesEnAttente      { get => _stats.SaisiesEnAttente; }
    public double TauxOccupation        { get => _stats.TauxOccupation; }
    public int    AccouchementsDuMois   { get => _stats.AccouchementsDuMois; }
    public int    CesariennesDuMois     { get => _stats.CesariennesDuMois; }
    public int    MapeAlertes           { get => _stats.MapeAlertes; }
    public double TauxCesarienne        => AccouchementsDuMois > 0
        ? Math.Round((double)CesariennesDuMois / AccouchementsDuMois * 100, 1) : 0;

    // Graphiques
    public ObservableCollection<ChartBar>   ConsultationsChart   { get; } = new();
    public ObservableCollection<ChartBar>   HospParServiceChart  { get; } = new();
    public ObservableCollection<ChartBar>   MapeChart            { get; } = new();
    public ObservableCollection<ChartSlice> GenreChart           { get; } = new();
    public ObservableCollection<MapeAlerte> AlertesMape          { get; } = new();

    // Propriétés booléennes pour IsVisible (Avalonia ne supporte pas !Count directement)
    public bool HasConsultationsData   => ConsultationsChart.Any(c => c.Value > 0);
    public bool HasHospData            => HospParServiceChart.Count > 0;
    public bool HasGenreData           => GenreChart.Count > 0;
    public bool NoGenreData            => GenreChart.Count == 0;
    public bool HasMapeData            => MapeChart.Count > 0;
    public bool NoMapeData             => MapeChart.Count == 0;
    public bool HasAlertesMape         => AlertesMape.Count > 0;
    public bool NoAlertesMape          => AlertesMape.Count == 0;

    public ICommand ActualiserCommand { get; }

    public async Task LoadAsync() => await ChargerAsync();

    private async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var debut = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fin   = DateTime.Now;

            _stats.ConsultationsDuMois = await _context.SaisiesConsultation
                .Where(s => s.DateSaisie >= debut && s.DateSaisie <= fin && s.Validee)
                .SumAsync(s => s.NouveauxHommes + s.NouvellesFemmes + s.NouveauxEnfants
                             + s.AnciensHommes  + s.AnciennesFemmes + s.AnciensEnfants);

            // v5: Track nouveaux patients
            _stats.NouveauxPatientsMois = await _context.SaisiesConsultation
                .Where(s => s.DateSaisie >= debut && s.DateSaisie <= fin && s.Validee)
                .SumAsync(s => s.NouveauxHommes + s.NouvellesFemmes + s.NouveauxEnfants);

            _stats.HospitalisationsTotal = await _context.SaisiesHospitalisation
                .Where(h => h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee)
                .SumAsync(h => h.Hommes + h.Femmes + h.Enfants);

            // v5: Décès provenant de l'hôpital (distinctes des corps externes)
            _stats.DecesDuMoisHopital = await _context.SaisiesMorgue
                .Where(m => m.DateSaisie >= debut && m.DateSaisie <= fin && m.Validee)
                .SumAsync(m => m.CasFromHopital);

            // Total décès (legacy)
            _stats.DecesDuMois = await _context.SaisiesDeces
                .Where(d => d.DateSaisie >= debut && d.DateSaisie <= fin && d.Validee)
                .SumAsync(d => d.Hommes + d.Femmes + d.Enfants);

            _stats.SaisiesEnAttente =
                await _context.SaisiesConsultation.CountAsync(s => !s.Validee) +
                await _context.SaisiesHospitalisation.CountAsync(s => !s.Validee) +
                await _context.SaisiesActeChirurgical.CountAsync(s => !s.Validee);

            var accouches = await _context.SaisiesHospitalisation
                .Where(h => h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee
                         && (h.TypeActivite.Contains("accouchement") || h.TypeActivite.Contains("Accouchement")))
                .ToListAsync();
            _stats.AccouchementsDuMois = accouches.Sum(a => a.Total);
            _stats.CesariennesDuMois   = accouches.Where(a => a.TypeActivite.Contains("sarienn")).Sum(a => a.Total);

            var totalLits = await _context.Services.Where(s => s.Actif && s.CapaciteLits.HasValue).SumAsync(s => s.CapaciteLits ?? 0);
            var joursHosp = await _context.SaisiesHospitalisation
                .Where(h => h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee)
                .SumAsync(h => h.JoursHospitalisation);
            var jours = Math.Max(1, (fin - debut).Days + 1);
            _stats.TauxOccupation = totalLits > 0 ? Math.Round((double)joursHosp / (totalLits * jours) * 100, 1) : 0;

            var derniereMape = await _context.SaisiesMape
                .Include(m => m.Lignes)
                .OrderByDescending(m => m.AnneeEpi)
                .ThenByDescending(m => m.SemaineEpi)
                .FirstOrDefaultAsync();
            _stats.MapeAlertes = derniereMape?.Lignes.Count(l => l.TotalCasSuspects > 0) ?? 0;

            NotifyAllKpis();

            await ChargerGraphiqueConsultations();
            await ChargerGraphiqueHospitalisations(debut, fin);
            ChargerGraphiqueMape(derniereMape);
            await ChargerCamembertGenre(debut, fin);
            ChargerAlertesMape(derniereMape);

            // Notifier les booléens de visibilité
            NotifyVisibility();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard erreur: {ex.Message}\n{ex.StackTrace}");
        }
        finally { IsLoading = false; }
    }

    private async Task ChargerGraphiqueConsultations()
    {
        ConsultationsChart.Clear();
        int maxVal = 1;
        var vals = new List<(string Mois, int Total)>();

        for (int i = 5; i >= 0; i--)
        {
            var d = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
            var f = d.AddMonths(1).AddDays(-1);
            var total = await _context.SaisiesConsultation
                .Where(s => s.DateSaisie >= d && s.DateSaisie <= f && s.Validee)
                .SumAsync(s => s.NouveauxHommes + s.NouvellesFemmes + s.NouveauxEnfants
                             + s.AnciensHommes  + s.AnciennesFemmes + s.AnciensEnfants);
            vals.Add((d.ToString("MMM"), total));
            if (total > maxVal) maxVal = total;
        }

        foreach (var (mois, total) in vals)
            ConsultationsChart.Add(new ChartBar { Label = mois, Value = total, MaxValue = maxVal, Color = "#005BA1" });
    }

    private async Task ChargerGraphiqueHospitalisations(DateTime debut, DateTime fin)
    {
        HospParServiceChart.Clear();
        var data = await _context.SaisiesHospitalisation
            .Include(h => h.Service)
            .Where(h => h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee)
            .GroupBy(h => h.Service!.Libelle)
            .Select(g => new { Service = g.Key, Total = g.Sum(x => x.Hommes + x.Femmes + x.Enfants) })
            .OrderByDescending(x => x.Total)
            .Take(6)
            .ToListAsync();

        int max = data.Any() ? data.Max(d => d.Total) : 1;
        string[] colors = { "#005BA1", "#00A4BD", "#059669", "#D97706", "#7C3AED", "#DC2626" };
        for (int i = 0; i < data.Count; i++)
            HospParServiceChart.Add(new ChartBar
            {
                Label    = data[i].Service.Length > 14 ? data[i].Service[..14] + "…" : data[i].Service,
                Value    = data[i].Total,
                MaxValue = max,
                Color    = colors[i % colors.Length]
            });
    }

    private void ChargerGraphiqueMape(SaisieMape? mape)
    {
        MapeChart.Clear();
        if (mape == null) return;
        var top5 = mape.Lignes.Where(l => l.TotalCasSuspects > 0)
                              .OrderByDescending(l => l.TotalCasSuspects).Take(5).ToList();
        int max = top5.Any() ? top5.Max(l => l.TotalCasSuspects) : 1;
        foreach (var l in top5)
            MapeChart.Add(new ChartBar
            {
                Label    = l.Maladie.Length > 18 ? l.Maladie[..18] + "…" : l.Maladie,
                Value    = l.TotalCasSuspects,
                MaxValue = max,
                Color    = l.EstUrgence ? "#DC2626" : "#D97706"
            });
    }

    private async Task ChargerCamembertGenre(DateTime debut, DateTime fin)
    {
        GenreChart.Clear();
        var consults = await _context.SaisiesConsultation
            .Where(s => s.DateSaisie >= debut && s.DateSaisie <= fin && s.Validee)
            .ToListAsync();

        int totalH = consults.Sum(c => c.NouveauxHommes  + c.AnciensHommes);
        int totalF = consults.Sum(c => c.NouvellesFemmes + c.AnciennesFemmes);
        int totalE = consults.Sum(c => c.NouveauxEnfants + c.AnciensEnfants);
        int grand  = totalH + totalF + totalE;

        if (grand > 0)
        {
            GenreChart.Add(new ChartSlice { Label = "Hommes",  Value = totalH, Percent = Math.Round((double)totalH / grand * 100, 1), Color = "#005BA1" });
            GenreChart.Add(new ChartSlice { Label = "Femmes",  Value = totalF, Percent = Math.Round((double)totalF / grand * 100, 1), Color = "#00A4BD" });
            GenreChart.Add(new ChartSlice { Label = "Enfants", Value = totalE, Percent = Math.Round((double)totalE / grand * 100, 1), Color = "#34D399" });
        }
    }

    private void ChargerAlertesMape(SaisieMape? mape)
    {
        // PAS de Task.Run — ObservableCollection doit être modifiée sur le UI thread
        AlertesMape.Clear();
        if (mape == null) return;
        foreach (var l in mape.Lignes.Where(l => l.TotalCasSuspects > 0).OrderByDescending(l => l.TotalCasSuspects))
            AlertesMape.Add(new MapeAlerte
            {
                Maladie     = l.Maladie,
                CasSuspects = l.TotalCasSuspects,
                Deces       = l.TotalDeces,
                Confirmes   = l.CasConfirmes,
                EstUrgente  = l.EstUrgence,
                Semaine     = $"S{mape.SemaineEpi}/{mape.AnneeEpi}"
            });
    }

    private void NotifyAllKpis()
    {
        OnPropertyChanged(nameof(ConsultationsDuMois));   OnPropertyChanged(nameof(NouveauxPatientsMois));    // v5
        OnPropertyChanged(nameof(HospitalisationsTotal)); OnPropertyChanged(nameof(DecesDuMois));
        OnPropertyChanged(nameof(DecesDuMoisHopital));    OnPropertyChanged(nameof(SaisiesEnAttente));         // v5
        OnPropertyChanged(nameof(TauxOccupation));        OnPropertyChanged(nameof(AccouchementsDuMois));
        OnPropertyChanged(nameof(CesariennesDuMois));     OnPropertyChanged(nameof(TauxCesarienne));
        OnPropertyChanged(nameof(MapeAlertes));
    }

    private void NotifyVisibility()
    {
        OnPropertyChanged(nameof(HasConsultationsData)); OnPropertyChanged(nameof(HasHospData));
        OnPropertyChanged(nameof(HasGenreData));         OnPropertyChanged(nameof(NoGenreData));
        OnPropertyChanged(nameof(HasMapeData));          OnPropertyChanged(nameof(NoMapeData));
        OnPropertyChanged(nameof(HasAlertesMape));       OnPropertyChanged(nameof(NoAlertesMape));
    }
}

// ===== Classes graphiques =====
public class ChartBar
{
    public string Label    { get; set; } = string.Empty;
    public int    Value    { get; set; }
    public int    MaxValue { get; set; } = 100;
    public string Color    { get; set; } = "#005BA1";
    // Retourne une valeur 0-1 pour la ProgressBar (Max=1.0)
    public double Percent  => MaxValue > 0 ? Math.Min((double)Value / MaxValue, 1.0) : 0;
}

public class ChartSlice
{
    public string Label   { get; set; } = string.Empty;
    public int    Value   { get; set; }
    public double Percent { get; set; }
    public string Color   { get; set; } = "#005BA1";
    public string Display => $"{Label} : {Value} ({Percent:0.#}%)";
}

public class MapeAlerte
{
    public string Maladie     { get; set; } = string.Empty;
    public int    CasSuspects { get; set; }
    public int    Deces       { get; set; }
    public int    Confirmes   { get; set; }
    public bool   EstUrgente  { get; set; }
    public string Semaine     { get; set; } = string.Empty;
    public string NiveauColor => EstUrgente ? "#FEE2E2" : "#FEF3C7";
    public string NiveauText  => EstUrgente ? "URGENT" : "Surveillance";
    public string NiveauFg    => EstUrgente ? "#DC2626" : "#D97706";
}

internal class DashboardStats
{
    public int    ConsultationsDuMois   { get; set; }
    public int    NouveauxPatientsMois  { get; set; }  // v5
    public int    HospitalisationsTotal { get; set; }
    public int    DecesDuMois           { get; set; }
    public int    DecesDuMoisHopital    { get; set; }  // v5: hôpital uniquement
    public int    SaisiesEnAttente      { get; set; }
    public double TauxOccupation        { get; set; }
    public int    AccouchementsDuMois   { get; set; }
    public int    CesariennesDuMois     { get; set; }
    public int    MapeAlertes           { get; set; }
}
