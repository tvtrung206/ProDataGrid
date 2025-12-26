// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace Avalonia.Controls
{
    /// <summary>
    /// Base class for column summary definitions.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    abstract class DataGridSummaryDescription : AvaloniaObject
    {
        /// <summary>
        /// Identifies the <see cref="Scope"/> property.
        /// </summary>
        public static readonly StyledProperty<DataGridSummaryScope> ScopeProperty =
            AvaloniaProperty.Register<DataGridSummaryDescription, DataGridSummaryScope>(
                nameof(Scope),
                defaultValue: DataGridSummaryScope.Total);

        /// <summary>
        /// Identifies the <see cref="StringFormat"/> property.
        /// </summary>
        public static readonly StyledProperty<string?> StringFormatProperty =
            AvaloniaProperty.Register<DataGridSummaryDescription, string?>(nameof(StringFormat));

        /// <summary>
        /// Identifies the <see cref="Converter"/> property.
        /// </summary>
        public static readonly StyledProperty<IValueConverter?> ConverterProperty =
            AvaloniaProperty.Register<DataGridSummaryDescription, IValueConverter?>(nameof(Converter));

        /// <summary>
        /// Identifies the <see cref="ConverterParameter"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> ConverterParameterProperty =
            AvaloniaProperty.Register<DataGridSummaryDescription, object?>(nameof(ConverterParameter));

        /// <summary>
        /// Identifies the <see cref="Title"/> property.
        /// </summary>
        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<DataGridSummaryDescription, string?>(nameof(Title));

        /// <summary>
        /// Identifies the <see cref="ContentTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
            AvaloniaProperty.Register<DataGridSummaryDescription, IDataTemplate?>(nameof(ContentTemplate));

        /// <summary>
        /// Gets or sets the scope of this summary (Total, Group, or Both).
        /// </summary>
        public DataGridSummaryScope Scope
        {
            get => GetValue(ScopeProperty);
            set => SetValue(ScopeProperty, value);
        }

        /// <summary>
        /// Gets or sets the display format string.
        /// </summary>
        public string? StringFormat
        {
            get => GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }

        /// <summary>
        /// Gets or sets the value converter.
        /// </summary>
        public IValueConverter? Converter
        {
            get => GetValue(ConverterProperty);
            set => SetValue(ConverterProperty, value);
        }

        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object? ConverterParameter
        {
            get => GetValue(ConverterParameterProperty);
            set => SetValue(ConverterParameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the title/label for this summary.
        /// </summary>
        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the content template for displaying the summary value.
        /// </summary>
        public IDataTemplate? ContentTemplate
        {
            get => GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        /// <summary>
        /// Calculates the summary value for the given items.
        /// </summary>
        /// <param name="items">The items to summarize.</param>
        /// <param name="column">The column being summarized.</param>
        /// <returns>The calculated summary value.</returns>
        public abstract object? Calculate(IEnumerable items, DataGridColumn column);

        /// <summary>
        /// Formats the calculated value for display.
        /// </summary>
        /// <param name="value">The calculated value.</param>
        /// <param name="culture">The culture to use for formatting.</param>
        /// <returns>The formatted string.</returns>
        public virtual string? FormatValue(object? value, CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;

            // Apply converter if specified
            if (Converter != null)
            {
                value = Converter.Convert(value, typeof(string), ConverterParameter, culture);
            }

            string formattedValue = string.Empty;
            var hasFormattedValue = false;

            // Apply string format
            if (!string.IsNullOrEmpty(StringFormat))
            {
                try
                {
                    // Handle format strings with and without placeholders
                    if (StringFormat.Contains("{0"))
                    {
                        formattedValue = string.Format(culture, StringFormat, value);
                        hasFormattedValue = true;
                    }
                    else if (value is IFormattable formattable)
                    {
                        formattedValue = formattable.ToString(StringFormat, culture);
                        hasFormattedValue = true;
                    }
                }
                catch
                {
                    // If formatting fails, fall through to default
                }
            }

            if (!hasFormattedValue)
            {
                formattedValue = value?.ToString() ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(Title))
            {
                if (string.IsNullOrEmpty(formattedValue))
                {
                    return Title;
                }

                if (char.IsWhiteSpace(Title[Title.Length - 1]))
                {
                    return $"{Title}{formattedValue}";
                }

                return $"{Title} {formattedValue}";
            }

            return formattedValue;
        }

        /// <summary>
        /// Gets the aggregate type for this summary description.
        /// </summary>
        public abstract DataGridAggregateType AggregateType { get; }

        /// <summary>
        /// Applies to the specified scope.
        /// </summary>
        /// <param name="scope">The scope to check.</param>
        /// <returns>True if this summary applies to the scope.</returns>
        public bool AppliesTo(DataGridSummaryScope scope)
        {
            return Scope == DataGridSummaryScope.Both || Scope == scope;
        }
    }
}
