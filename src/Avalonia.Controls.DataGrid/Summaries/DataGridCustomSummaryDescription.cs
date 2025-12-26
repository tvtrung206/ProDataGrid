// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace Avalonia.Controls
{
    /// <summary>
    /// Summary description with custom calculation logic.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    class DataGridCustomSummaryDescription : DataGridSummaryDescription
    {
        /// <summary>
        /// Identifies the <see cref="Calculator"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataGridSummaryCalculator?> CalculatorProperty =
            AvaloniaProperty.Register<DataGridCustomSummaryDescription, IDataGridSummaryCalculator?>(nameof(Calculator));

        private Func<IEnumerable, DataGridColumn, object?>? _calculateFunc;

        /// <summary>
        /// Gets or sets the custom calculator instance.
        /// </summary>
        public IDataGridSummaryCalculator? Calculator
        {
            get => GetValue(CalculatorProperty);
            set => SetValue(CalculatorProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom calculation function.
        /// </summary>
        public Func<IEnumerable, DataGridColumn, object?>? CalculateFunc
        {
            get => _calculateFunc;
            set => _calculateFunc = value;
        }

        /// <inheritdoc/>
        public override DataGridAggregateType AggregateType => DataGridAggregateType.Custom;

        /// <inheritdoc/>
        public override object? Calculate(IEnumerable items, DataGridColumn column)
        {
            // Prefer the function if set
            if (CalculateFunc != null)
            {
                return CalculateFunc(items, column);
            }

            // Otherwise use the calculator
            if (Calculator != null)
            {
                var propertyName = GetPropertyName(column);
                return Calculator.Calculate(items, column, propertyName);
            }

            return null;
        }

        private static string? GetPropertyName(DataGridColumn column)
        {
            var propertyName = column.GetSortPropertyName();
            return string.IsNullOrWhiteSpace(propertyName) ? null : propertyName;
        }
    }
}
