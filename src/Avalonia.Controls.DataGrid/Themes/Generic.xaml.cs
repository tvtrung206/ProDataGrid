using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Controls.DataGridThemes;

public partial class DataGridGenericTheme : Styles
{
    public DataGridGenericTheme()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
