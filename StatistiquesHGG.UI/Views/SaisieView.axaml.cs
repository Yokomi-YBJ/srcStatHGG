using Avalonia.Controls;

namespace StatistiquesHGG.UI.Views;

public partial class SaisieView : UserControl
{
    public SaisieView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += async (s, e) =>
        {
            if (this.DataContext is ILoadable loadable)
                await loadable.LoadAsync();
        };
    }
}
