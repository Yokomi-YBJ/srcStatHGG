using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;
using StatistiquesHGG.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StatistiquesHGG.UI;

public class SaisieViewModel : BaseViewModel, ILoadable
{
    private readonly AppDbContext _context;
    private int _selectedTypeIndex = 0;
    private Service? _selectedService;
    private string _statusMessage = string.Empty;
    private bool _isSuccess = false;

    private readonly AuthenticationService _authService;

    public SaisieViewModel(AppDbContext context, AuthenticationService authService)
    {
        _context = context;
        _authService = authService;
        Services = new ObservableCollection<Service>();
        DernieresSaisies = new ObservableCollection<SaisieHospitalisation>();

        GenreOptions = new[] { "Masculin", "Féminin", "Enfant" };
        OrientationOptions = new[] { "Retour domicile", "Hospitalisation", "Transfert", "Décès" };
        ModeSortieOptions = new[] { "Guérison", "Décès", "Transfert", "Évasion", "Autres" };
        TypeAccouchementOptions = new[] { "Voie basse", "Césarienne" };
        TypesImagerie = new[] { "Radio", "Échographie", "Scanner", "IRM" };
        TypesReeducation = new[]
        {
            "Rééducation para faciale", "Rééducation épaule", "Rééducation genou",
            "Rééducation hémiplégie", "Rééducation membre inférieur", "Rééducation membre supérieur",
            "Rééducation respiratoire", "Rééducation VIP", "Rééducation paroi abdominale",
            "Rééducation paraplégie", "Rééducation cheville", "Rééducation coude",
            "Rééducation poignet", "Rééducation plusieurs membres", "Paralysie plexus brachial",
            "Rééducation rachis", "Rééducation tétraplégie"
        };

        EnregistrerCommand = new RelayCommand(async () => await EnregistrerAsync());
        ReinitialiserCommand = new RelayCommandSync(Reinitialiser);
    }

    public ObservableCollection<Service> Services { get; }
    public ObservableCollection<SaisieHospitalisation> DernieresSaisies { get; }

    public string[] GenreOptions { get; }
    public string[] OrientationOptions { get; }
    public string[] ModeSortieOptions { get; }
    public string[] TypeAccouchementOptions { get; }
    public string[] TypesImagerie { get; }
    public string[] TypesReeducation { get; }

    public string[] TypesDonnee => new[]
    {
        "Consultation", "Hospitalisation", "Accouchement",
        "Examen Laboratoire", "Examen Imagerie", "Réhabilitation"
    };

    public int SelectedTypeIndex
    {
        get => _selectedTypeIndex;
        set
        {
            SetProperty(ref _selectedTypeIndex, value);
            OnPropertyChanged(nameof(IsConsultation));
            OnPropertyChanged(nameof(IsHospitalisation));
            OnPropertyChanged(nameof(IsAccouchement));
            OnPropertyChanged(nameof(IsExamenLabo));
            OnPropertyChanged(nameof(IsExamenImagerie));
            OnPropertyChanged(nameof(IsRehabilitation));
        }
    }

    public bool IsConsultation => SelectedTypeIndex == 0;
    public bool IsHospitalisation => SelectedTypeIndex == 1;
    public bool IsAccouchement => SelectedTypeIndex == 2;
    public bool IsExamenLabo => SelectedTypeIndex == 3;
    public bool IsExamenImagerie => SelectedTypeIndex == 4;
    public bool IsRehabilitation => SelectedTypeIndex == 5;

    public Service? SelectedService { get => _selectedService; set => SetProperty(ref _selectedService, value); }

    // Consultation fields
    public string ConsultMotif { get; set; } = string.Empty;
    public string ConsultDiagnostic { get; set; } = string.Empty;
    public int ConsultGenreIndex { get; set; } = 0;
    public int? ConsultAge { get; set; }
    public bool EstNouvelleConsultation { get; set; } = true;
    public int ConsultOrientationIndex { get; set; } = 0;

    // Hospitalisation fields
    public DateTimeOffset HospDateAdmission { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset? HospDateSortie { get; set; }
    public string HospDiagnostic { get; set; } = string.Empty;
    public int HospGenreIndex { get; set; } = 0;
    public int? HospAge { get; set; }
    public int HospModeSortieIndex { get; set; } = 0;

    // Accouchement fields
    public int AccTypeIndex { get; set; } = 0;
    public int AccGenreNNIndex { get; set; } = 0;
    public decimal? AccPoids { get; set; }
    public string AccComplications { get; set; } = string.Empty;
    public int AccDureeSejour { get; set; } = 2;

    // Labo fields
    public string LaboTypeExamen { get; set; } = string.Empty;
    public string LaboResultat { get; set; } = string.Empty;

    // Imagerie fields
    public int ImgTypeIndex { get; set; } = 0;
    public string ImgResultat { get; set; } = string.Empty;

    // Rééducation fields
    public int RehabTypeIndex { get; set; } = 0;
    public int RehabGenreIndex { get; set; } = 0;
    public int RehabNbSeances { get; set; } = 1;

    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool IsSuccess { get => _isSuccess; set => SetProperty(ref _isSuccess, value); }

    public ICommand EnregistrerCommand { get; }
    public ICommand ReinitialiserCommand { get; }

    public async Task LoadAsync()
    {
        var services = await _context.Services.Where(s => s.Actif).OrderBy(s => s.Libelle).ToListAsync();
        Services.Clear();
        foreach (var s in services) Services.Add(s);

        var user = AuthenticationService.CurrentUser;
        if (user?.ServiceId != null && user.Role != Core.Enums.RoleType.SuperAdmin)
            SelectedService = Services.FirstOrDefault(s => s.Id == user.ServiceId);

        await LoadDernieresSaisies();
    }

    private async Task LoadDernieresSaisies()
    {
        var user = AuthenticationService.CurrentUser;
        IQueryable<SaisieHospitalisation> query = _context.SaisiesHospitalisation
            .Include(d => d.Service)
            .Include(d => d.SaisiePar);

        if (user?.Role == Core.Enums.RoleType.AgentDeSaisie && user.ServiceId.HasValue)
            query = query.Where(d => d.ServiceId == user.ServiceId && d.SaisieParId == user.Id);

        var items = await query.OrderByDescending(d => d.DateSaisie).Take(20).ToListAsync();
        DernieresSaisies.Clear();
        foreach (var item in items) DernieresSaisies.Add(item);
    }

    private async Task EnregistrerAsync()
    {
        if (SelectedService == null) { SetStatus("Veuillez sélectionner un service.", false); return; }
        var user = AuthenticationService.CurrentUser;
        if (user == null) { SetStatus("Utilisateur non connecté.", false); return; }

        try
        {
            // Enregistrer l'hospitali selon le type sélectionné
            switch (SelectedTypeIndex)
            {
                case 1: // Hospitalisation
                    var saisieH = new SaisieHospitalisation
                    {
                        ServiceId = SelectedService.Id,
                        TypeActivite = HospDiagnostic,
                        DateSaisie = DateTime.Now,
                        SaisieParId = user.Id,
                        Validee = false,
                        Hommes = HospGenreIndex == 0 ? 1 : 0,
                        Femmes = HospGenreIndex == 1 ? 1 : 0,
                        Enfants = HospGenreIndex == 2 ? 1 : 0,
                        JoursHospitalisation = HospDateSortie.HasValue ? (HospDateSortie.Value.DateTime - HospDateAdmission.DateTime).Days : 0
                    };
                    _context.SaisiesHospitalisation.Add(saisieH);
                    break;

                case 0: // Consultation
                    var saisieC = new SaisieConsultation
                    {
                        ServiceId = SelectedService.Id,
                        DateSaisie = DateTime.Now,
                        SaisieParId = user.Id,
                        Validee = false,
                        NouveauxHommes = ConsultGenreIndex == 0 && EstNouvelleConsultation ? 1 : 0,
                        NouvellesFemmes = ConsultGenreIndex == 1 && EstNouvelleConsultation ? 1 : 0,
                        NouveauxEnfants = ConsultGenreIndex == 2 && EstNouvelleConsultation ? 1 : 0,
                        AnciensHommes = ConsultGenreIndex == 0 && !EstNouvelleConsultation ? 1 : 0,
                        AnciennesFemmes = ConsultGenreIndex == 1 && !EstNouvelleConsultation ? 1 : 0,
                        AnciensEnfants = ConsultGenreIndex == 2 && !EstNouvelleConsultation ? 1 : 0
                    };
                    _context.SaisiesConsultation.Add(saisieC);
                    break;

                case 4: // Imagerie
                    var saisieImg = new SaisieHospitalisation
                    {
                        ServiceId = SelectedService.Id,
                        TypeActivite = $"Imagerie - {TypesImagerie[ImgTypeIndex]}",
                        DateSaisie = DateTime.Now,
                        SaisieParId = user.Id,
                        Validee = false
                    };
                    _context.SaisiesHospitalisation.Add(saisieImg);
                    break;

                default:
                    SetStatus("Type de saisie non supporté pour maintenant.", false);
                    return;
            }

            await _context.SaveChangesAsync();
            SetStatus("Donnée enregistrée avec succès.", true);
            Reinitialiser();
            await LoadDernieresSaisies();
        }
        catch (Exception ex)
        {
            SetStatus($"Erreur : {ex.Message}", false);
        }
    }

    private void Reinitialiser()
    {
        ConsultMotif = string.Empty; OnPropertyChanged(nameof(ConsultMotif));
        ConsultDiagnostic = string.Empty; OnPropertyChanged(nameof(ConsultDiagnostic));
        ConsultAge = null; OnPropertyChanged(nameof(ConsultAge));
        HospDiagnostic = string.Empty; OnPropertyChanged(nameof(HospDiagnostic));
        HospAge = null; OnPropertyChanged(nameof(HospAge));
        LaboTypeExamen = string.Empty; OnPropertyChanged(nameof(LaboTypeExamen));
        LaboResultat = string.Empty; OnPropertyChanged(nameof(LaboResultat));
        ImgResultat = string.Empty; OnPropertyChanged(nameof(ImgResultat));
        StatusMessage = string.Empty;
    }

    private void SetStatus(string msg, bool success)
    {
        StatusMessage = msg;
        IsSuccess = success;
    }
}
