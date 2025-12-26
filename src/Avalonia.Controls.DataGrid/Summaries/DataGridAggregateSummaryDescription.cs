// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Avalonia.Controls
{
    /// <summary>
    /// Summary description using built-in aggregate functions.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    class DataGridAggregateSummaryDescription : DataGridSummaryDescription
    {
        private static readonly DataGridSummaryCalculatorFactory _calculatorFactory = new();

        /// <summary>
        /// Identifies the <see cref="Aggregate"/> property.
        /// </summary>
        public static readonly StyledProperty<DataGridAggregateType> AggregateProperty =
            AvaloniaProperty.Register<DataGridAggregateSummaryDescription, DataGridAggregateType>(
                nameof(Aggregate),
                defaultValue: DataGridAggregateType.None);

        /// <summary>
        /// Gets or sets the aggregate function type.
        /// </summary>
        public DataGridAggregateType Aggregate
        {
            get => GetValue(AggregateProperty);
            set => SetValue(AggregateProperty, value);
        }

        /// <inheritdoc/>
        public override DataGridAggregateType AggregateType => Aggregate;

        /// <inheritdoc/>
        public override object? Calculate(IEnumerable items, DataGridColumn column)
        {
            if (Aggregate == DataGridAggregateType.None)
            {
                return null;
            }

            var calculator = _calculatorFactory.GetCalculator(Aggregate);
            if (calculator == null)
            {
                return null;
            }

            var propertyName = GetPropertyName(column);
            return calculator.Calculate(items, column, propertyName);
        }

        private static string? GetPropertyName(DataGridColumn column)
        {
            var propertyName = column.GetSortPropertyName();
            return string.IsNullOrWhiteSpace(propertyName) ? null : propertyName;
        }
    }
}
