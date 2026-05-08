using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;
using StatistiquesHGG.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StatistiquesHGG.UI;

public class ValidationViewModel : BaseViewModel, ILoadable
{
    private readonly AppDbContext _context;
    private readonly AuthenticationService _authService;
    private string _statusMessage = string.Empty;
    private bool _isSuccess;
    private int _totalEnAttente;

    public ValidationViewModel(AppDbContext context, AuthenticationService authService)
    {
        _context = context;
        _authService = authService;
        DonneesEnAttente = new ObservableCollection<SaisieEnAttenteItem>();

        ValiderCommand    = new RelayCommand(async () => await ValiderSelectionAsync());
        RejeterCommand    = new RelayCommand(async () => await RejeterSelectionAsync());
        ActualiserCommand = new RelayCommand(async () => await ChargerDonneesAsync());
        ValiderToutCommand = new RelayCommand(async () => await ValiderToutAsync());
    }

    public ObservableCollection<SaisieEnAttenteItem> DonneesEnAttente { get; }

    public SaisieEnAttenteItem? SelectedItem { get; set; }
    public bool HasSelection => SelectedItem != null;
    public string CommentaireValidation { get; set; } = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool   IsSuccess     { get => _isSuccess;     set => SetProperty(ref _isSuccess, value); }
    public int    TotalEnAttente { get => _totalEnAttente; set => SetProperty(ref _totalEnAttente, value); }

    public ICommand ValiderCommand     { get; }
    public ICommand RejeterCommand     { get; }
    public ICommand ActualiserCommand  { get; }
    public ICommand ValiderToutCommand { get; }

    public async Task LoadAsync() => await ChargerDonneesAsync();

    private async Task ChargerDonneesAsync()
    {
        var user = AuthenticationService.CurrentUser;
        if (user == null) return;

        DonneesEnAttente.Clear();
        var items = new List<SaisieEnAttenteItem>();

        // Consultations en attente
        IQueryable<SaisieConsultation> qC = _context.SaisiesConsultation
            .Include(s => s.Service).Include(s => s.SaisiePar)
            .Where(s => !s.Validee);
        if (user.Role == RoleType.ChefDeSaisie && user.ServiceId.HasValue)
            qC = qC.Where(s => s.ServiceId == user.ServiceId);
        var consults = await qC.OrderBy(s => s.DateSaisie).Take(50).ToListAsync();
        items.AddRange(consults.Select(c => new SaisieEnAttenteItem
        {
            Id = c.Id, Type = "Consultation", TypeCle = "CONSULT",
            Service = c.Service?.Libelle ?? "-",
            SaisiePar = c.SaisiePar?.NomComplet ?? "-",
            DateSaisie = c.DateSaisie,
            Resume = $"N: {c.TotalNouveaux} | A: {c.TotalAnciens} | Tot: {c.TotalGeneral}"
        }));

        // Hospitalisations en attente
        IQueryable<SaisieHospitalisation> qH = _context.SaisiesHospitalisation
            .Include(s => s.Service).Include(s => s.SaisiePar)
            .Where(s => !s.Validee);
        if (user.Role == RoleType.ChefDeSaisie && user.ServiceId.HasValue)
            qH = qH.Where(s => s.ServiceId == user.ServiceId);
        var hosps = await qH.OrderBy(s => s.DateSaisie).Take(50).ToListAsync();
        items.AddRange(hosps.Select(h => new SaisieEnAttenteItem
        {
            Id = h.Id, Type = "Hospitalisation", TypeCle = "HOSP",
            Service = h.Service?.Libelle ?? "-",
            SaisiePar = h.SaisiePar?.NomComplet ?? "-",
            DateSaisie = h.DateSaisie,
            Resume = $"{h.TypeActivite} | H:{h.Hommes} F:{h.Femmes} E:{h.Enfants} | {h.JoursHospitalisation}j"
        }));

        // Actes chirurgicaux en attente
        IQueryable<SaisieActeChirurgical> qChir = _context.SaisiesActeChirurgical
            .Include(s => s.Service).Include(s => s.SaisiePar)
            .Where(s => !s.Validee);
        if (user.Role == RoleType.ChefDeSaisie && user.ServiceId.HasValue)
            qChir = qChir.Where(s => s.ServiceId == user.ServiceId);
        var chirs = await qChir.OrderBy(s => s.DateSaisie).Take(30).ToListAsync();
        items.AddRange(chirs.Select(c => new SaisieEnAttenteItem
        {
            Id = c.Id, Type = "Chirurgie", TypeCle = "CHIR",
            Service = c.Service?.Libelle ?? "-",
            SaisiePar = c.SaisiePar?.NomComplet ?? "-",
            DateSaisie = c.DateSaisie,
            Resume = $"{c.TypeActe} | H:{c.Hommes} F:{c.Femmes} E:{c.Enfants}"
        }));

        // Décès en attente
        IQueryable<SaisieDeces> qD = _context.SaisiesDeces
            .Include(s => s.Service).Include(s => s.SaisiePar)
            .Where(s => !s.Validee);
        if (user.Role == RoleType.ChefDeSaisie && user.ServiceId.HasValue)
            qD = qD.Where(s => s.ServiceId == user.ServiceId);
        var deces = await qD.OrderBy(s => s.DateSaisie).Take(30).ToListAsync();
        items.AddRange(deces.Select(d => new SaisieEnAttenteItem
        {
            Id = d.Id, Type = "Décès", TypeCle = "DECES",
            Service = d.Service?.Libelle ?? "-",
            SaisiePar = d.SaisiePar?.NomComplet ?? "-",
            DateSaisie = d.DateSaisie,
            Resume = $"H:{d.Hommes} F:{d.Femmes} E:{d.Enfants} | Total:{d.Total}"
        }));

        // MAPE en attente
        var mapes = await _context.SaisiesMape
            .Include(m => m.SaisiePar)
            .Where(m => !m.Validee)
            .OrderBy(m => m.DateEnregistrement)
            .Take(20)
            .ToListAsync();
        items.AddRange(mapes.Select(m => new SaisieEnAttenteItem
        {
            Id = m.Id, Type = "MAPE", TypeCle = "MAPE",
            Service = "Épidémiologie",
            SaisiePar = m.SaisiePar?.NomComplet ?? "-",
            DateSaisie = m.DateEnregistrement,
            Resume = $"Semaine {m.SemaineEpi}/{m.AnneeEpi} — {m.DateDebut:dd/MM} au {m.DateFin:dd/MM}"
        }));

        foreach (var item in items.OrderBy(x => x.DateSaisie))
            DonneesEnAttente.Add(item);

        TotalEnAttente = DonneesEnAttente.Count;
    }

    private async Task ValiderSelectionAsync()
    {
        if (SelectedItem == null) return;
        await ValiderItem(SelectedItem);
        DonneesEnAttente.Remove(SelectedItem);
        SelectedItem = null;
        TotalEnAttente--;
        SetStatus($"Donnée validée avec succès.", true);
    }

    private async Task RejeterSelectionAsync()
    {
        if (SelectedItem == null) return;
        SetStatus($"Donnée #{SelectedItem.Id} ({SelectedItem.Type}) rejetée.", true);
        DonneesEnAttente.Remove(SelectedItem);
        SelectedItem = null;
        TotalEnAttente--;
        await Task.Delay(0);
    }

    private async Task ValiderToutAsync()
    {
        var user = AuthenticationService.CurrentUser;
        if (user == null) return;

        foreach (var item in DonneesEnAttente.ToList())
            await ValiderItem(item);

        int count = DonneesEnAttente.Count;
        DonneesEnAttente.Clear();
        TotalEnAttente = 0;
        SetStatus($"{count} saisies validées.", true);
    }

    private async Task ValiderItem(SaisieEnAttenteItem item)
    {
        var user = AuthenticationService.CurrentUser;
        if (user == null) return;
        var now = DateTime.Now;

        switch (item.TypeCle)
        {
            case "CONSULT":
                var c = await _context.SaisiesConsultation.FindAsync(item.Id);
                if (c != null) { c.Validee = true; c.ValideeParId = user.Id; c.DateValidation = now; }
                break;
            case "HOSP":
                var h = await _context.SaisiesHospitalisation.FindAsync(item.Id);
                if (h != null) { h.Validee = true; h.ValideeParId = user.Id; h.DateValidation = now; }
                break;
            case "CHIR":
                var ch = await _context.SaisiesActeChirurgical.FindAsync(item.Id);
                if (ch != null) { ch.Validee = true; ch.ValideeParId = user.Id; ch.DateValidation = now; }
                break;
            case "DECES":
                var d = await _context.SaisiesDeces.FindAsync(item.Id);
                if (d != null) { d.Validee = true; d.ValideeParId = user.Id; d.DateValidation = now; }
                break;
            case "MAPE":
                var m = await _context.SaisiesMape.FindAsync(item.Id);
                if (m != null) { m.Validee = true; m.ValideeParId = user.Id; m.DateValidation = now; }
                break;
        }
        await _context.SaveChangesAsync();
    }

    private void SetStatus(string msg, bool ok) { StatusMessage = msg; IsSuccess = ok; }
}

public class SaisieEnAttenteItem
{
    public int      Id        { get; set; }
    public string   Type      { get; set; } = string.Empty;
    public string   TypeCle   { get; set; } = string.Empty;
    public string   Service   { get; set; } = string.Empty;
    public string   SaisiePar { get; set; } = string.Empty;
    public DateTime DateSaisie { get; set; }
    public string   Resume    { get; set; } = string.Empty;
    public string   TypeColor => TypeCle switch
    {
        "CONSULT" => "#E0F2FE",
        "HOSP"    => "#F0FDF4",
        "CHIR"    => "#FEF3C7",
        "DECES"   => "#FEE2E2",
        "MAPE"    => "#FDF4FF",
        _         => "#F1F5F9"
    };
}
