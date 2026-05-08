using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using StatistiquesHGG.Business.Services;
using StatistiquesHGG.UI;

namespace StatistiquesHGG.UI.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is LoginViewModel vm)
        {
            vm.LoginSucceeded += OnLoginSucceeded;
        }
    }

    private void OnLoginSucceeded()
    {
        var mainVm = App.Services.GetRequiredService<MainViewModel>();
        var mainWindow = new MainWindow { DataContext = mainVm };
        mainVm.LogoutRequested += () =>
        {
            App.Services.GetRequiredService<AuthenticationService>().Logout();
            var loginVm = App.Services.GetRequiredService<LoginViewModel>();
            var loginWindow = new LoginWindow { DataContext = loginVm };
            loginWindow.Show();
            mainWindow.Close();
        };
        mainWindow.Show();
        Close();
    }
}
