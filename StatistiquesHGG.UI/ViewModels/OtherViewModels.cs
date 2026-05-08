using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;
using StatistiquesHGG.Infrastructure.Data;
using StatistiquesHGG.Reporting.Generators;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StatistiquesHGG.UI;

// ========================= RAPPORT VIEW MODEL =========================
public class RapportViewModel : BaseViewModel, ILoadable
{
    private readonly AppDbContext _context;
    private readonly RMAGenerator _rmaGenerator;
    private string _statusMessage = string.Empty;
    private bool _isSuccess;
    private bool _isGenerating;

    public RapportViewModel(AppDbContext context, RMAGenerator rmaGenerator)
    {
        _context = context;
        _rmaGenerator = rmaGenerator;
        Rapports = new ObservableCollection<Rapport>();
        SelectedPeriode = DateTimeOffset.Now;
        TypeRapportIndex = 0; // RMA par défaut

        GenererRMAExcelCommand = new RelayCommand(async () => await GenererRMAAsync("Excel"));
        GenererRMAPDFCommand   = new RelayCommand(async () => await GenererRMAAsync("PDF"));
        OuvrirDossierCommand   = new RelayCommandSync(OuvrirDossier);
    }

    public ObservableCollection<Rapport> Rapports        { get; }
    public DateTimeOffset  SelectedPeriode               { get; set; }
    public int             TypeRapportIndex              { get; set; }
    public string          StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool            IsSuccess     { get => _isSuccess;     set => SetProperty(ref _isSuccess, value); }
    public bool            IsGenerating  { get => _isGenerating;  set => SetProperty(ref _isGenerating, value); }

    public string[] TypesRapport => new[] { "RMA — Mensuel", "MAPE — Hebdomadaire", "DHIS2" };

    public ICommand GenererRMAExcelCommand { get; }
    public ICommand GenererRMAPDFCommand   { get; }
    public ICommand OuvrirDossierCommand   { get; }

    public async Task LoadAsync()
    {
        var rapports = await _context.Rapports
            .Include(r => r.GenerePar)
            .OrderByDescending(r => r.DateGeneration)
            .Take(30).ToListAsync();
        Rapports.Clear();
        foreach (var r in rapports) Rapports.Add(r);
    }

    private async Task GenererRMAAsync(string format)
    {
        IsGenerating = true;
        StatusMessage = $"Génération du RMA en cours ({format})...";
        try
        {
            var periode = SelectedPeriode.DateTime;
            var dossier = AppSettings.ReportsFolder;
            string fichier = format == "Excel"
                ? await _rmaGenerator.GenererExcelAsync(periode, dossier)
                : await _rmaGenerator.GenererPDFAsync(periode, dossier);

            var user = AuthenticationService.CurrentUser;
            var rapport = new Rapport
            {
                Type = StatistiquesHGG.Core.Enums.TypeRapport.RMA,
                Titre = $"RMA — {periode:MMMM yyyy}",
                Periode = new DateTime(periode.Year, periode.Month, 1),
                GenereParId = user!.Id,
                DateGeneration = DateTime.Now,
                CheminFichier = fichier
            };
            _context.Rapports.Add(rapport);
            await _context.SaveChangesAsync();

            await LoadAsync();
            SetStatus($"Rapport généré : {System.IO.Path.GetFileName(fichier)}", true);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            { FileName = fichier, UseShellExecute = true });
        }
        catch (Exception ex) { SetStatus($"Erreur : {ex.Message}", false); }
        finally { IsGenerating = false; }
    }

    private void OuvrirDossier()
    {
        var dossier = AppSettings.ReportsFolder;
        System.IO.Directory.CreateDirectory(dossier);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        { FileName = dossier, UseShellExecute = true });
    }

    private void SetStatus(string msg, bool ok) { StatusMessage = msg; IsSuccess = ok; }
}

// ========================= CLASSEMENT VIEW MODEL =========================
public class ClassementViewModel : BaseViewModel, ILoadable
{
    private readonly StatistiquesService _statsService;
    private bool _isLoading;

    public ClassementViewModel(StatistiquesService statsService)
    {
        _statsService = statsService;
        Classement = new ObservableCollection<ScorePerformance>();
        SelectedPeriode = DateTimeOffset.Now;
        ActualiserCommand  = new RelayCommand(async () => await ChargerClassementAsync());
        RecalculerCommand  = new RelayCommand(async () => await RecalculerAsync());
    }

    public ObservableCollection<ScorePerformance> Classement { get; }
    public DateTimeOffset SelectedPeriode { get; set; }
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public ICommand ActualiserCommand { get; }
    public ICommand RecalculerCommand { get; }

    public async Task LoadAsync() => await ChargerClassementAsync();

    private async Task ChargerClassementAsync()
    {
        IsLoading = true;
        try
        {
            var scores = await _statsService.GetClassementServicesAsync(SelectedPeriode.DateTime);
            Classement.Clear();
            foreach (var s in scores) Classement.Add(s);
        }
        catch { }
        finally { IsLoading = false; }
    }

    private async Task RecalculerAsync()
    {
        IsLoading = true;
        try
        {
            await _statsService.RecalculerScoresAsync(SelectedPeriode.DateTime);
            await ChargerClassementAsync();
        }
        catch { }
        finally { IsLoading = false; }
    }
}

// ========================= UTILISATEURS VIEW MODEL =========================
public class UtilisateursViewModel : BaseViewModel, ILoadable
{
    private readonly AppDbContext _context;
    private Utilisateur? _selectedUser;
    private string _statusMessage = string.Empty;
    private bool _isSuccess;
    private bool _isEditing;
    private bool _isNouvelUtilisateur;

    public UtilisateursViewModel(AppDbContext context)
    {
        _context = context;
        Utilisateurs = new ObservableCollection<Utilisateur>();
        Services     = new ObservableCollection<Service>();
        RoleOptions  = new[] { "Super Admin", "Major de Service", "Directeur/DPM", "Agent de Saisie" };

        NouveauCommand    = new RelayCommandSync(NouvelUtilisateur);
        EnregistrerCommand = new RelayCommand(async () => await EnregistrerAsync());
        SupprimerCommand   = new RelayCommand(async () => await SupprimerAsync());
        AnnulerCommand     = new RelayCommandSync(Annuler);
        GenererMDPCommand  = new RelayCommandSync(GenererMotDePasse);
    }

    public ObservableCollection<Utilisateur> Utilisateurs { get; }
    public ObservableCollection<Service>     Services     { get; }
    public string[] RoleOptions { get; }

    public Utilisateur? SelectedUser
    {
        get => _selectedUser;
        set
        {
            SetProperty(ref _selectedUser, value);
            if (value != null) ChargerFormulaire(value);
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection        => SelectedUser != null;
    public bool IsEditing           { get => _isEditing;           set => SetProperty(ref _isEditing, value); }
    public bool IsNouvelUtilisateur { get => _isNouvelUtilisateur; set => SetProperty(ref _isNouvelUtilisateur, value); }

    public string FormTitre => IsNouvelUtilisateur ? "Nouvel utilisateur" : "Modifier utilisateur";

    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool   IsSuccess     { get => _isSuccess;     set => SetProperty(ref _isSuccess, value); }

    // Champs formulaire
    private string _formLogin = string.Empty;
    private string _formNom = string.Empty;
    private string _formPrenom = string.Empty;
    private string _formEmail = string.Empty;
    private string _formMotDePasse = string.Empty;
    private int _formRoleIndex = 3;
    private bool _formActif = true;
    private Service? _formService;

    public string FormLogin      { get => _formLogin;      set => SetProperty(ref _formLogin,      value); }
    public string FormNom        { get => _formNom;        set => SetProperty(ref _formNom,        value); }
    public string FormPrenom     { get => _formPrenom;     set => SetProperty(ref _formPrenom,     value); }
    public string FormEmail      { get => _formEmail;      set => SetProperty(ref _formEmail,      value); }
    public string FormMotDePasse { get => _formMotDePasse; set => SetProperty(ref _formMotDePasse, value); }
    public int    FormRoleIndex  { get => _formRoleIndex;  set => SetProperty(ref _formRoleIndex,  value); }
    public bool   FormActif      { get => _formActif;      set => SetProperty(ref _formActif,      value); }
    public Service? FormService
    {
        get => _formService;
        set => SetProperty(ref _formService, value);
    }

    public ICommand NouveauCommand     { get; }
    public ICommand EnregistrerCommand { get; }
    public ICommand SupprimerCommand   { get; }
    public ICommand AnnulerCommand     { get; }
    public ICommand GenererMDPCommand  { get; }

    public async Task LoadAsync()
    {
        var users = await _context.Utilisateurs
            .Include(u => u.Service)
            .OrderBy(u => u.Nom)
            .ToListAsync();
        Utilisateurs.Clear();
        foreach (var u in users) Utilisateurs.Add(u);

        var services = await _context.Services
            .Where(s => s.Actif).OrderBy(s => s.Libelle).ToListAsync();
        Services.Clear();
        // Ajouter option "Aucun service"
        Services.Add(new Service { Id = 0, Code = "NONE", Libelle = "Aucun service" });
        foreach (var s in services) Services.Add(s);
    }

    private void NouvelUtilisateur()
    {
        _selectedUser = null;
        OnPropertyChanged(nameof(SelectedUser));
        OnPropertyChanged(nameof(HasSelection));

        FormLogin      = string.Empty;
        FormNom        = string.Empty;
        FormPrenom     = string.Empty;
        FormEmail      = string.Empty;
        FormMotDePasse = AuthenticationService.GeneratePassword();
        FormRoleIndex  = 3;
        FormService    = Services.FirstOrDefault(s => s.Id == 0);
        FormActif      = true;
        IsNouvelUtilisateur = true;
        IsEditing      = true;
        OnPropertyChanged(nameof(FormTitre));
        StatusMessage  = string.Empty;
    }

    private void ChargerFormulaire(Utilisateur u)
    {
        FormLogin      = u.Login;
        FormNom        = u.Nom;
        FormPrenom     = u.Prenom;
        FormEmail      = u.Email ?? string.Empty;
        FormMotDePasse = string.Empty;
        FormRoleIndex  = (int)u.Role;
        FormActif      = u.Actif;
        FormService    = u.ServiceId.HasValue
            ? Services.FirstOrDefault(s => s.Id == u.ServiceId)
            : Services.FirstOrDefault(s => s.Id == 0);
        IsNouvelUtilisateur = false;
        IsEditing      = true;
        OnPropertyChanged(nameof(FormTitre));
        StatusMessage  = string.Empty;
    }

    private async Task EnregistrerAsync()
    {
        if (string.IsNullOrWhiteSpace(FormLogin) || string.IsNullOrWhiteSpace(FormNom) || string.IsNullOrWhiteSpace(FormPrenom))
        {
            SetStatus("Login, nom et prénom sont obligatoires.", false);
            return;
        }

        try
        {
            int? serviceId = (FormService == null || FormService.Id == 0) ? null : FormService.Id;

            if (IsNouvelUtilisateur)
            {
                if (await _context.Utilisateurs.AnyAsync(u => u.Login == FormLogin))
                {
                    SetStatus($"Le login '{FormLogin}' est déjà utilisé.", false);
                    return;
                }
                if (string.IsNullOrWhiteSpace(FormMotDePasse))
                {
                    SetStatus("Le mot de passe est obligatoire pour un nouvel utilisateur.", false);
                    return;
                }

                var newUser = new Utilisateur
                {
                    Login          = FormLogin,
                    Nom            = FormNom,
                    Prenom         = FormPrenom,
                    Email          = FormEmail,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(FormMotDePasse),
                    Role           = (RoleType)FormRoleIndex,
                    ServiceId      = serviceId,
                    Actif          = FormActif,
                    DateCreation   = DateTime.Now
                };
                _context.Utilisateurs.Add(newUser);
                SetStatus($"Utilisateur '{FormLogin}' créé. MDP : {FormMotDePasse}", true);
            }
            else if (SelectedUser != null)
            {
                SelectedUser.Nom       = FormNom;
                SelectedUser.Prenom    = FormPrenom;
                SelectedUser.Email     = FormEmail;
                SelectedUser.Role      = (RoleType)FormRoleIndex;
                SelectedUser.ServiceId = serviceId;
                SelectedUser.Actif     = FormActif;
                if (!string.IsNullOrWhiteSpace(FormMotDePasse))
                    SelectedUser.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(FormMotDePasse);
                SetStatus($"Utilisateur '{SelectedUser.Login}' mis à jour.", true);
            }

            await _context.SaveChangesAsync();
            await LoadAsync();
            IsEditing = false;
        }
        catch (Exception ex)
        {
            SetStatus($"Erreur : {ex.Message} | {ex.InnerException?.Message}", false);
        }
    }

    private async Task SupprimerAsync()
    {
        if (SelectedUser == null) return;
        if (SelectedUser.Role == RoleType.SuperAdmin)
        {
            SetStatus("Impossible de supprimer le super admin.", false);
            return;
        }
        try
        {
            SelectedUser.Actif = false;
            await _context.SaveChangesAsync();
            SetStatus("Compte désactivé.", true);
            await LoadAsync();
            _selectedUser = null;
            IsEditing = false;
            OnPropertyChanged(nameof(SelectedUser));
            OnPropertyChanged(nameof(HasSelection));
        }
        catch (Exception ex) { SetStatus($"Erreur : {ex.Message}", false); }
    }

    private void Annuler()
    {
        IsEditing     = false;
        _selectedUser = null;
        OnPropertyChanged(nameof(SelectedUser));
        OnPropertyChanged(nameof(HasSelection));
        StatusMessage = string.Empty;
    }

    private void GenererMotDePasse()
    {
        FormMotDePasse = AuthenticationService.GeneratePassword();
    }

    private void SetStatus(string msg, bool ok) { StatusMessage = msg; IsSuccess = ok; }
}
