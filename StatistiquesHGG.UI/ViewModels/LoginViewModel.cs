using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StatistiquesHGG.Business.Services;

namespace StatistiquesHGG.UI;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthenticationService _authService;
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? LoginSucceeded;

    public LoginViewModel(AuthenticationService authService)
    {
        _authService = authService;
        LoginCommand = new RelayCommand(async () => await ExecuteLoginAsync(), () => !IsLoading);
    }

    public string Login
    {
        get => _login;
        set { _login = value; OnPropertyChanged(); ClearError(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); ClearError(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }

    private async Task ExecuteLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Veuillez saisir votre identifiant et mot de passe.";
            return;
        }
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var (success, message, _) = await _authService.LoginAsync(Login, Password);
            if (success)
                LoginSucceeded?.Invoke();
            else
                ErrorMessage = message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de connexion à la base de données.\nVérifiez la configuration XAMPP.\n{ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearError() => ErrorMessage = string.Empty;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
