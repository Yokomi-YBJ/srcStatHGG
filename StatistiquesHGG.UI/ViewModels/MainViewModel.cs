using System.Windows.Input;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.Core.Enums;

namespace StatistiquesHGG.UI;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel? _currentPage;
    private string _activePage = "Dashboard";
    private readonly IServiceProvider _services;
    private readonly AuthenticationService _authService;

    public MainViewModel(IServiceProvider services, AuthenticationService authService)
    {
        _services = services;
        _authService = authService;
        var user = AuthenticationService.CurrentUser;
        UserName = user?.NomComplet ?? "Utilisateur";
        UserRole = user?.Role switch
        {
            RoleType.SuperAdmin    => "Super Administrateur",
            RoleType.Consulteur    => "Consulteur (Lecture seule)",
            RoleType.ChefDeSaisie  => $"Chef de Saisie — {user.Service?.Libelle ?? "Service"}",
            RoleType.AgentDeSaisie => $"Agent de Saisie — {user.Service?.Libelle ?? "Service"}",
            _                      => "Utilisateur"
        };

        // v5 RBAC: Permissions selon le rôle
        CanSaisir   = user?.Role is RoleType.SuperAdmin or RoleType.ChefDeSaisie or RoleType.AgentDeSaisie;
        CanValider  = user?.Role is RoleType.SuperAdmin or RoleType.ChefDeSaisie;
        CanRapports = user?.Role is RoleType.SuperAdmin; // Rapports SuperAdmin uniquement
        CanAdmin    = user?.Role == RoleType.SuperAdmin;
        CanViewPerformances = user?.Role is RoleType.SuperAdmin or RoleType.ChefDeSaisie;
        // v5: Dashboard visible uniquement par SuperAdmin et Consulteur
        CanViewDashboard = user?.Role is RoleType.SuperAdmin or RoleType.Consulteur;

        NavigateCommand = new RelayCommandSync<object?>(page => NavigateTo(page?.ToString() ?? "Dashboard"));
        NavigateTo("Dashboard");
    }

    public string UserName  { get; }
    public string UserRole  { get; }
    public bool CanSaisir   { get; }
    public bool CanValider  { get; }
    public bool CanRapports { get; }
    public bool CanAdmin    { get; }
    public bool CanViewPerformances { get; }
    public bool CanViewDashboard { get; }

    public string ActivePage
    {
        get => _activePage;
        set { SetProperty(ref _activePage, value); NotifyNavActiveChanged(); }
    }

    public BaseViewModel? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public ICommand NavigateCommand { get; }
    public event Action? LogoutRequested;

    public bool IsDashboardActive  => ActivePage == "Dashboard";
    public bool IsSaisieActive     => ActivePage == "Saisie";
    public bool IsMouvementActive  => ActivePage == "Mouvement";
    // v5: Menu Validation supprimé définitivement
    public bool IsRapportsActive   => ActivePage == "Rapports";
    public bool IsClassementActive => ActivePage == "Classement";
    public bool IsAdminActive      => ActivePage == "Admin";
    public bool IsCiblesActive     => ActivePage == "Cibles";

    public void NavigateTo(string page)
    {
        // v5: Dashboard uniquement pour SuperAdmin et Consulteur
        if (page == "Dashboard" && !CanViewDashboard)
        {
            page = CanSaisir ? "Saisie" : "Classement";
        }
        
        BaseViewModel? newPage = page switch
        {
            "Dashboard"     => GetService<DashboardViewModel>(),
            "Saisie"        => GetService<SaisieRmaViewModel>(),
            "Mouvement"     => GetService<MouvementPatientViewModel>(),
            // v5: Validation supprimé — navigation rejetée
            "Validation"    => GetService<DashboardViewModel>(),  // Redirect vers Dashboard
            "Rapports"      => GetService<RapportViewModel>(),
            "Classement"    => GetService<ClassementViewModel>(),
            "Admin"         => GetService<UtilisateursViewModel>(),
            "Cibles"        => GetService<CiblesViewModel>(),
            _               => GetService<DashboardViewModel>()
        };

        CurrentPage = newPage;
        ActivePage  = page;

        if (newPage is ILoadable loadable)
        {
            _ = loadable.LoadAsync(); // Fire and forget, or use await in an async context
        }
    }

    public void Logout() => LogoutRequested?.Invoke();

    private T GetService<T>() where T : notnull
        => (T)_services.GetService(typeof(T))!;

    private void NotifyNavActiveChanged()
    {
        OnPropertyChanged(nameof(IsDashboardActive));
        OnPropertyChanged(nameof(IsSaisieActive));
        OnPropertyChanged(nameof(IsMouvementActive));
        // v5: Suppression notification IsValidationActive
        OnPropertyChanged(nameof(IsRapportsActive));
        OnPropertyChanged(nameof(IsClassementActive));
        OnPropertyChanged(nameof(IsAdminActive));
        OnPropertyChanged(nameof(IsCiblesActive));
    }
}

// ILoadable avec Task pour support async
public interface ILoadable
{
    Task LoadAsync();
}
