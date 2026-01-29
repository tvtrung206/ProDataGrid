using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.DataGridThemes;

public partial class DataGridSimpleTheme : Styles
{
    public DataGridSimpleTheme()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
