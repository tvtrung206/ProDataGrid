// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Avalonia.Controls.DataGridFormulas
{
    internal sealed class DataGridFormulaMultiValueConverter : IMultiValueConverter
    {
        private readonly DataGrid _grid;
        private readonly DataGridFormulaColumnDefinition _column;

        public DataGridFormulaMultiValueConverter(DataGrid grid, DataGridFormulaColumnDefinition column)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0)
            {
                return null;
            }

            var item = values[0];
            if (item == null || item == AvaloniaProperty.UnsetValue)
            {
                return null;
            }

            var model = _grid.FormulaModel;
            if (model == null)
            {
                return null;
            }

            var result = model.Evaluate(item, _column);
            if (result == null || targetType == null || targetType.IsInstanceOfType(result))
            {
                return result;
            }

            return DataGridValueConverter.Instance.Convert(result, targetType, parameter, culture);
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            var result = new object[targetTypes.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = BindingOperations.DoNothing;
            }

            return result;
        }
    }
}
