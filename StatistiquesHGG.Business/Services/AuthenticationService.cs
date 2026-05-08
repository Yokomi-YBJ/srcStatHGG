using Microsoft.EntityFrameworkCore;
using StatistiquesHGG.Core.Entities;
using StatistiquesHGG.Core.Enums;
using StatistiquesHGG.Infrastructure.Data;

namespace StatistiquesHGG.Business.Services;

public class AuthenticationService
{
    private readonly AppDbContext _context;
    private static Utilisateur? _currentUser;

    public AuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public static Utilisateur? CurrentUser => _currentUser;
    public static bool IsAuthenticated => _currentUser != null;

    public async Task<(bool Success, string Message, Utilisateur? User)> LoginAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return (false, "Login et mot de passe requis.", null);

        var user = await _context.Utilisateurs
            .Include(u => u.Service)
            .FirstOrDefaultAsync(u => u.Login == login && u.Actif);

        if (user == null)
            return (false, "Identifiants incorrects.", null);

        bool valid = BCrypt.Net.BCrypt.Verify(password, user.MotDePasseHash);
        if (!valid)
            return (false, "Identifiants incorrects.", null);

        // Mettre à jour DerniereConnexion — séparé du log pour éviter
        // qu'une erreur de log bloque la connexion
        try
        {
            user.DerniereConnexion = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Si la mise à jour échoue, on laisse passer quand même
            _context.Entry(user).State = EntityState.Unchanged;
        }

        // Log de connexion — silencieux en cas d'erreur
        try
        {
            _context.LogsActions.Add(new LogAction
            {
                UtilisateurId    = user.Id,
                Action           = "CONNEXION",
                TableCible       = "Utilisateur",
                EnregistrementId = user.Id,
                DateAction       = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Log non critique — ne bloque pas la connexion
        }

        _currentUser = user;
        return (true, "Connexion réussie.", user);
    }

    public void Logout()
    {
        _currentUser = null;
    }

    public bool HasPermission(RoleType requiredRole)
    {
        if (_currentUser == null) return false;
        return _currentUser.Role <= requiredRole;
    }

    public static bool CanManageUsers()    => _currentUser?.Role == RoleType.SuperAdmin;
    public static bool CanValidateData()   => _currentUser?.Role == RoleType.SuperAdmin || _currentUser?.Role == RoleType.ChefDeSaisie;
    public static bool CanGenerateReports()=> _currentUser?.Role == RoleType.SuperAdmin || _currentUser?.Role == RoleType.Consulteur;
    public static bool CanViewDashboard()  => _currentUser != null;
    public static bool IsSuperAdmin()      => _currentUser?.Role == RoleType.SuperAdmin;
    public static bool IsDirecteur()       => _currentUser?.Role == RoleType.Consulteur;

    public static string GeneratePassword()
    {
        var adjectives = new[] { "Bleu", "Rouge", "Fort", "Sage", "Grand",
                                 "Vif",  "Doux", "Noir", "Haut", "Clair" };
        var nouns      = new[] { "Lion", "Aigle", "Arbre", "Soleil", "Mer",
                                 "Tigre","Fleuve","Roche","Nuage", "Feu"  };
        var rand = new Random();
        var adj  = adjectives[rand.Next(adjectives.Length)];
        var noun = nouns[rand.Next(nouns.Length)];
        var num  = rand.Next(10, 99);
        return $"{adj}{noun}{num}!";
    }
}
