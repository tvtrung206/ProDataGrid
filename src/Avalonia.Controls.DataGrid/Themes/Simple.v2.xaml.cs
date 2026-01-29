using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.DataGridThemes;

public partial class DataGridSimpleV2Theme : Styles
{
    public DataGridSimpleV2Theme()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
