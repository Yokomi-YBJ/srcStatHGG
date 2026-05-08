using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StatistiquesHGG.UI;

public class CiblesViewModel : BaseViewModel, ILoadable
{
    private readonly AppDbContext _context;
    private string _statusMessage = string.Empty;
    private bool _isSuccess;
    private CibleIndicateur? _selectedCible;

    public CiblesViewModel(AppDbContext context)
    {
        _context = context;
        Cibles   = new ObservableCollection<CibleIndicateur>();
        Services = new ObservableCollection<Service>();
        Indicateurs = new ObservableCollection<Indicateur>();

        EnregistrerCommand = new RelayCommand(async () => await EnregistrerAsync());
        SupprimerCommand   = new RelayCommand(async () => await SupprimerAsync());
        NouveauCommand     = new RelayCommandSync(Nouveau);

        PeriodeOptions = new[] { "MENSUEL", "ANNUEL" };
    }

    public ObservableCollection<CibleIndicateur> Cibles { get; }
    public ObservableCollection<Service>         Services { get; }
    public ObservableCollection<Indicateur>      Indicateurs { get; }
    public string[] PeriodeOptions { get; }

    public CibleIndicateur? SelectedCible
    {
        get => _selectedCible;
        set { SetProperty(ref _selectedCible, value); ChargerFormulaire(value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool HasSelection => SelectedCible != null;
    public bool IsEditing    { get; private set; }

    // Champs formulaire
    public Service?    FormService    { get; set; }
    public Indicateur? FormIndicateur { get; set; }
    public decimal     FormValeurCible { get; set; } = 100;
    public int         FormPeriodeIndex { get; set; } = 0;
    public bool        FormActif { get; set; } = true;

    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool   IsSuccess     { get => _isSuccess;     set => SetProperty(ref _isSuccess, value); }

    public ICommand EnregistrerCommand { get; }
    public ICommand SupprimerCommand   { get; }
    public ICommand NouveauCommand     { get; }

    public async Task LoadAsync()
    {
        var cibles = await _context.CiblesIndicateurs
            .Include(c => c.Service)
            .Include(c => c.Indicateur)
            .OrderBy(c => c.Service!.Libelle)
            .ToListAsync();
        Cibles.Clear();
        foreach (var c in cibles) Cibles.Add(c);

        var services = await _context.Services.Where(s => s.Actif).OrderBy(s => s.Libelle).ToListAsync();
        Services.Clear();
        foreach (var s in services) Services.Add(s);

        var indicateurs = await _context.Indicateurs.OrderBy(i => i.Libelle).ToListAsync();
        Indicateurs.Clear();
        foreach (var i in indicateurs) Indicateurs.Add(i);
    }

    private void Nouveau()
    {
        SelectedCible   = null;
        FormService     = null;
        FormIndicateur  = null;
        FormValeurCible = 100;
        FormPeriodeIndex = 0;
        FormActif       = true;
        IsEditing       = true;
        NotifyForm();
    }

    private void ChargerFormulaire(CibleIndicateur? c)
    {
        if (c == null) return;
        FormService      = Services.FirstOrDefault(s => s.Id == c.ServiceId);
        FormIndicateur   = Indicateurs.FirstOrDefault(i => i.Id == c.IndicateurId);
        FormValeurCible  = c.ValeurCible;
        FormPeriodeIndex = c.Periode == "ANNUEL" ? 1 : 0;
        FormActif        = c.Actif;
        IsEditing        = true;
        NotifyForm();
    }

    private async Task EnregistrerAsync()
    {
        if (FormService == null || FormIndicateur == null)
        { SetStatus("Service et indicateur sont obligatoires.", false); return; }

        try
        {
            if (SelectedCible == null)
            {
                var nouvelle = new CibleIndicateur
                {
                    ServiceId    = FormService.Id,
                    IndicateurId = FormIndicateur.Id,
                    ValeurCible  = FormValeurCible,
                    Periode      = PeriodeOptions[FormPeriodeIndex],
                    Actif        = FormActif,
                    DateDebut    = DateTime.Now
                };
                _context.CiblesIndicateurs.Add(nouvelle);
                SetStatus("Cible créée avec succès.", true);
            }
            else
            {
                SelectedCible.ServiceId    = FormService.Id;
                SelectedCible.IndicateurId = FormIndicateur.Id;
                SelectedCible.ValeurCible  = FormValeurCible;
                SelectedCible.Periode      = PeriodeOptions[FormPeriodeIndex];
                SelectedCible.Actif        = FormActif;
                SetStatus("Cible mise à jour.", true);
            }

            await _context.SaveChangesAsync();
            await LoadAsync();
            IsEditing = false;
        }
        catch (Exception ex)
        {
            SetStatus($"Erreur : {ex.Message}", false);
        }
    }

    private async Task SupprimerAsync()
    {
        if (SelectedCible == null) return;
        try
        {
            _context.CiblesIndicateurs.Remove(SelectedCible);
            await _context.SaveChangesAsync();
            SetStatus("Cible supprimée.", true);
            await LoadAsync();
            SelectedCible = null;
            IsEditing     = false;
        }
        catch (Exception ex) { SetStatus($"Erreur : {ex.Message}", false); }
    }

    private void SetStatus(string msg, bool ok) { StatusMessage = msg; IsSuccess = ok; }
    private void NotifyForm()
    {
        OnPropertyChanged(nameof(FormService));
        OnPropertyChanged(nameof(FormIndicateur));
        OnPropertyChanged(nameof(FormValeurCible));
        OnPropertyChanged(nameof(FormPeriodeIndex));
        OnPropertyChanged(nameof(FormActif));
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(HasSelection));
    }
}
