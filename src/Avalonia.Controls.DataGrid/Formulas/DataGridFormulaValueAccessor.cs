// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using Avalonia.Controls;

namespace Avalonia.Controls.DataGridFormulas
{
    internal sealed class DataGridFormulaValueAccessor : IDataGridColumnValueAccessor
    {
        private readonly DataGrid _grid;
        private readonly DataGridFormulaColumnDefinition _column;

        public DataGridFormulaValueAccessor(DataGrid grid, DataGridFormulaColumnDefinition column)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public Type ItemType => typeof(object);

        public Type ValueType => _column.ValueType ?? typeof(object);

        public bool CanWrite => false;

        public object? GetValue(object item)
        {
            if (item == null)
            {
                return null;
            }

            var model = _grid.FormulaModel;
            if (model == null)
            {
                return null;
            }

            return model.Evaluate(item, _column);
        }

        public void SetValue(object item, object value)
        {
            throw new InvalidOperationException("Formula columns are read-only.");
        }
    }
}
