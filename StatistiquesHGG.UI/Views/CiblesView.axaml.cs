using Avalonia.Controls;
using Avalonia.Interactivity;

namespace StatistiquesHGG.UI.Views;

public partial class CiblesView : UserControl
{
    public CiblesView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += async (s, e) =>
        {
            if (this.DataContext is ILoadable loadable)
                await loadable.LoadAsync();
        };
    }
}
