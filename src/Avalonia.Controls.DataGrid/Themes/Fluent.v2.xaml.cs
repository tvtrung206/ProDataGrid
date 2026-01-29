using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.DataGridThemes;

public partial class DataGridFluentV2Theme : Styles
{
    public DataGridFluentV2Theme()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
