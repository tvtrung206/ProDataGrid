using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Diagnostics.Screenshots;
using Xunit;

namespace Avalonia.Diagnostics.UnitTests;

public class PublicApiSurfaceTests
{
    [Fact]
    public void Assembly_Exports_Only_Expected_Public_Types()
    {
        var assembly = typeof(DevToolsExtensions).Assembly;
        var exportedTypes = assembly
            .GetExportedTypes()
            .Where(type => !IsCompiledAvaloniaXamlType(type))
            .ToArray();
        var allowedTypes = new List<Type>
        {
            typeof(DevToolsExtensions),
            typeof(VisualTreeDebug),
            typeof(DevToolsViewKind),
            typeof(HotKeyConfiguration),
            typeof(DevToolsOptions),
            typeof(IScreenshotHandler),
            typeof(BaseRenderToStreamHandler),
            typeof(FilePickerHandler)
        };

        var dataGridThemeTypes = new[]
        {
            "Avalonia.Controls.DataGridThemes.DataGridFluentTheme",
            "Avalonia.Controls.DataGridThemes.DataGridFluentV2Theme",
            "Avalonia.Controls.DataGridThemes.DataGridGenericTheme",
            "Avalonia.Controls.DataGridThemes.DataGridSimpleTheme",
            "Avalonia.Controls.DataGridThemes.DataGridSimpleV2Theme"
        };

        foreach (var typeName in dataGridThemeTypes)
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                allowedTypes.Add(type);
            }
        }

        var unexpected = exportedTypes
            .Except(allowedTypes)
            .OrderBy(type => type.FullName)
            .ToArray();
        var missing = allowedTypes
            .Except(exportedTypes)
            .OrderBy(type => type.FullName)
            .ToArray();

        Assert.True(unexpected.Length == 0, $"Unexpected public types: {string.Join(", ", unexpected.Select(type => type.FullName ?? type.Name))}");
        Assert.True(missing.Length == 0, $"Missing public types: {string.Join(", ", missing.Select(type => type.FullName ?? type.Name))}");
    }

    private static bool IsCompiledAvaloniaXamlType(Type type)
    {
        return string.Equals(type.Namespace, "CompiledAvaloniaXaml", StringComparison.Ordinal)
               && type.Name.StartsWith("!", StringComparison.Ordinal);
    }
}
