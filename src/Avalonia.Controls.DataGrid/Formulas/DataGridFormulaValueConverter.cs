// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Avalonia.Controls.DataGridFormulas
{
    internal sealed class DataGridFormulaValueConverter : IValueConverter
    {
        private readonly DataGrid _grid;
        private readonly DataGridFormulaColumnDefinition _column;

        public DataGridFormulaValueConverter(DataGrid grid, DataGridFormulaColumnDefinition column)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var model = _grid.FormulaModel;
            if (model == null)
            {
                return null;
            }

            return model.Evaluate(value, _column);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
