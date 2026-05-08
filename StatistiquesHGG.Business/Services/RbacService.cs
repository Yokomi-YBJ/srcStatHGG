using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;

namespace StatistiquesHGG.Business.Services;

/// <summary>
/// Service de gestion des permissions (RBAC) pour v5
/// Contrôle l'accès aux vues et fonctionnalités selon le rôle et le service
/// </summary>
public class RbacService
{
    private readonly Utilisateur? _currentUser;

    public RbacService()
    {
        _currentUser = AuthenticationService.CurrentUser;
    }

    /// <summary>
    /// Vérifie si l'utilisateur peut accéder à une vue (Consulteur: Dashboard uniquement)
    /// </summary>
    public bool CanAccessView(string viewName)
    {
        if (_currentUser == null) return false;

        return (_currentUser.Role, viewName) switch
        {
            // Consulteur: Dashboard UNIQUEMENT
            (RoleType.Consulteur, "Dashboard") => true,
            (RoleType.Consulteur, _) => false,

            // SuperAdmin: accès à tout
            (RoleType.SuperAdmin, _) => true,

            // ChefDeSaisie: Dashboard, Saisie RMA, MAPE, Performances
            (RoleType.ChefDeSaisie, "Dashboard") => true,
            (RoleType.ChefDeSaisie, "Saisie") => true,
            (RoleType.ChefDeSaisie, "Classement") => true,  // Performances
            (RoleType.ChefDeSaisie, "Mape") => true,
            (RoleType.ChefDeSaisie, _) => false,

            // AgentDeSaisie: Saisie RMA UNIQUEMENT
            (RoleType.AgentDeSaisie, "Dashboard") => true,
            (RoleType.AgentDeSaisie, "Saisie") => true,
            (RoleType.AgentDeSaisie, _) => false,

            _ => false
        };
    }

    /// <summary>
    /// Vérifie si le champ service doit être visible/éditable
    /// SuperAdmin peut choisir le service, autres voient celui assigné (caché)
    /// </summary>
    public bool CanChooseService()
    {
        return _currentUser?.Role == RoleType.SuperAdmin;
    }

    /// <summary>
    /// Retourne le service de l'utilisateur (restreint pour non-SuperAdmin)
    /// </summary>
    public int? GetUserService()
    {
        if (_currentUser?.Role == RoleType.SuperAdmin)
            return null;  // SuperAdmin voit tous

        return _currentUser?.ServiceId;
    }

    /// <summary>
    /// Vérifie si l'utilisateur peut valider les données
    /// </summary>
    public bool CanValidate()
    {
        return _currentUser?.Role is RoleType.SuperAdmin or RoleType.ChefDeSaisie;
    }

    /// <summary>
    /// Vérifie si l'utilisateur peut voir les rapports
    /// </summary>
    public bool CanViewReports()
    {
        return _currentUser?.Role == RoleType.SuperAdmin;
    }

    /// <summary>
    /// Vérifie si l'utilisateur peut voir les performances (classement services)
    /// </summary>
    public bool CanViewPerformances()
    {
        return _currentUser?.Role is RoleType.SuperAdmin or RoleType.ChefDeSaisie;
    }

    /// <summary>
    /// Vérifie si l'utilisateur peut entrer en saisie
    /// </summary>
    public bool CanEnterData()
    {
        return _currentUser?.Role is RoleType.SuperAdmin or RoleType.ChefDeSaisie or RoleType.AgentDeSaisie;
    }

    /// <summary>
    /// Vérifie si l'utilisateur peut administrer (gestion utilisateurs, cibles, etc.)
    /// </summary>
    public bool CanAdminister()
    {
        return _currentUser?.Role == RoleType.SuperAdmin;
    }

    /// <summary>
    /// Retourne les services accessibles (filtre selon rôle)
    /// </summary>
    public List<int> GetAccessibleServices(List<Service> allServices)
    {
        if (_currentUser?.Role == RoleType.SuperAdmin)
            return allServices.Select(s => s.Id).ToList();

        return _currentUser?.ServiceId.HasValue == true
            ? new List<int> { _currentUser.ServiceId.Value }
            : new List<int>();
    }

    /// <summary>
    /// Vérifie si un formulaire section doit être affiché selon le service
    /// (ex: pas "Césarienne" pour un service ambulatoire)
    /// </summary>
    public bool IsFormSectionVisible(string sectionName, int? serviceId)
    {
        if (_currentUser?.Role == RoleType.SuperAdmin)
            return true;  // SuperAdmin voit tout

        if (serviceId == null)
            return true;

        // TODO: Implémenter la logique par service si besoin
        // ex: if (sectionName == "Hospitalization" && IsAmbulatory(serviceId)) return false;
        return true;
    }
}
