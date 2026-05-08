using Avalonia.Controls;
using Avalonia.Interactivity;

namespace StatistiquesHGG.UI.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += async (s, e) =>
        {
            if (this.DataContext is ILoadable loadable)
                await loadable.LoadAsync();
        };
    }
}
