using Avalonia.Controls;
using Avalonia.Interactivity;

namespace StatistiquesHGG.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnLogoutClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Logout();
    }
}
