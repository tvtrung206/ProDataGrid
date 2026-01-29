using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.DataGridThemes;

public partial class DataGridFluentTheme : Styles
{
    public DataGridFluentTheme()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
