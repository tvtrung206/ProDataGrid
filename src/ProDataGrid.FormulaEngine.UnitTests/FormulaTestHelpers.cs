// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Tests
{
    internal sealed class TestWorkbook : IFormulaWorkbook, IFormulaNameProvider, IFormulaNameChangeNotifier, IFormulaWorkbookResolver
    {
        private readonly Dictionary<string, IFormulaWorksheet> _worksheets = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IFormulaWorkbook> _externalWorkbooks = new(StringComparer.OrdinalIgnoreCase);
        private readonly FormulaNameTable _names = new();

        public TestWorkbook(string name)
        {
            Name = name;
            Settings = new FormulaCalculationSettings();
            AddWorksheet("Sheet1");
        }

        public string Name { get; }

        public FormulaCalculationSettings Settings { get; }

        public IReadOnlyList<IFormulaWorksheet> Worksheets => new List<IFormulaWorksheet>(_worksheets.Values);

        public FormulaNameTable Names => _names;

        public event EventHandler<FormulaNameChangedEventArgs>? NameChanged
        {
            add => _names.NameChanged += value;
            remove => _names.NameChanged -= value;
        }

        public IFormulaWorksheet GetWorksheet(string name)
        {
            if (!_worksheets.TryGetValue(name, out var sheet))
            {
                throw new KeyNotFoundException($"Worksheet '{name}' not found.");
            }
            return sheet;
        }

        public IFormulaWorksheet AddWorksheet(string name)
        {
            var sheet = new TestWorksheet(name, this);
            _worksheets[name] = sheet;
            return sheet;
        }

        public void RenameWorksheet(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Sheet names are required.");
            }

            if (!_worksheets.TryGetValue(oldName, out var sheet))
            {
                throw new KeyNotFoundException($"Worksheet '{oldName}' not found.");
            }

            if (_worksheets.ContainsKey(newName))
            {
                throw new InvalidOperationException($"Worksheet '{newName}' already exists.");
            }

            if (sheet is TestWorksheet testSheet)
            {
                testSheet.Rename(newName);
            }

            _worksheets.Remove(oldName);
            _worksheets[newName] = sheet;
        }

        public void RegisterExternalWorkbook(string name, IFormulaWorkbook workbook)
        {
            _externalWorkbooks[name] = workbook;
        }

        public bool TryGetName(string name, out FormulaExpression expression)
        {
            return _names.TryGetName(name, out expression);
        }

        public bool TryGetWorkbook(string name, out IFormulaWorkbook workbook)
        {
            return _externalWorkbooks.TryGetValue(name, out workbook!);
        }
    }

    internal sealed class TestWorksheet : IFormulaWorksheet, IFormulaNameProvider, IFormulaNameChangeNotifier
    {
        private readonly Dictionary<(int Row, int Column), IFormulaCell> _cells = new();
        private readonly FormulaNameTable _names = new();
        private string _name;

        public TestWorksheet(string name, IFormulaWorkbook workbook)
        {
            _name = name;
            Workbook = workbook;
        }

        public string Name => _name;

        public IFormulaWorkbook Workbook { get; }

        public FormulaNameTable Names => _names;

        public event EventHandler<FormulaNameChangedEventArgs>? NameChanged
        {
            add => _names.NameChanged += value;
            remove => _names.NameChanged -= value;
        }

        public IFormulaCell GetCell(int row, int column)
        {
            if (!_cells.TryGetValue((row, column), out var cell))
            {
                cell = new TestCell(new FormulaCellAddress(Name, row, column));
                _cells[(row, column)] = cell;
            }

            return cell;
        }

        public bool TryGetCell(int row, int column, out IFormulaCell cell)
        {
            return _cells.TryGetValue((row, column), out cell!);
        }

        public bool TryGetName(string name, out FormulaExpression expression)
        {
            return _names.TryGetName(name, out expression);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Sheet name is required.", nameof(name));
            }

            if (string.Equals(_name, name, StringComparison.Ordinal))
            {
                return;
            }

            var updated = new Dictionary<(int Row, int Column), IFormulaCell>();
            foreach (var cell in _cells.Values)
            {
                if (cell is not TestCell testCell)
                {
                    continue;
                }

                var address = testCell.Address;
                var cloned = CloneCell(testCell, name, address.Row, address.Column);
                updated[(address.Row, address.Column)] = cloned;
            }

            _cells.Clear();
            foreach (var pair in updated)
            {
                _cells[pair.Key] = pair.Value;
            }

            _name = name;
        }

        public void InsertRows(int rowIndex, int count)
        {
            ApplyRowShift(rowIndex, count, isInsert: true);
        }

        public void DeleteRows(int rowIndex, int count)
        {
            ApplyRowShift(rowIndex, count, isInsert: false);
        }

        public void InsertColumns(int columnIndex, int count)
        {
            ApplyColumnShift(columnIndex, count, isInsert: true);
        }

        public void DeleteColumns(int columnIndex, int count)
        {
            ApplyColumnShift(columnIndex, count, isInsert: false);
        }

        private void ApplyRowShift(int rowIndex, int count, bool isInsert)
        {
            var updated = new Dictionary<(int Row, int Column), IFormulaCell>();
            foreach (var cell in _cells.Values)
            {
                if (cell is not TestCell testCell)
                {
                    continue;
                }

                var row = testCell.Address.Row;
                var column = testCell.Address.Column;
                if (!TryShiftIndex(row, rowIndex, count, isInsert, out var shiftedRow))
                {
                    continue;
                }

                var cloned = CloneCell(testCell, _name, shiftedRow, column);
                updated[(shiftedRow, column)] = cloned;
            }

            _cells.Clear();
            foreach (var pair in updated)
            {
                _cells[pair.Key] = pair.Value;
            }
        }

        private void ApplyColumnShift(int columnIndex, int count, bool isInsert)
        {
            var updated = new Dictionary<(int Row, int Column), IFormulaCell>();
            foreach (var cell in _cells.Values)
            {
                if (cell is not TestCell testCell)
                {
                    continue;
                }

                var row = testCell.Address.Row;
                var column = testCell.Address.Column;
                if (!TryShiftIndex(column, columnIndex, count, isInsert, out var shiftedColumn))
                {
                    continue;
                }

                var cloned = CloneCell(testCell, _name, row, shiftedColumn);
                updated[(row, shiftedColumn)] = cloned;
            }

            _cells.Clear();
            foreach (var pair in updated)
            {
                _cells[pair.Key] = pair.Value;
            }
        }

        private static bool TryShiftIndex(int value, int index, int count, bool isInsert, out int shifted)
        {
            shifted = value;
            if (count <= 0)
            {
                return true;
            }

            if (isInsert)
            {
                if (value >= index)
                {
                    shifted = value + count;
                }
                return true;
            }

            var deleteEnd = index + count - 1;
            if (value >= index && value <= deleteEnd)
            {
                return false;
            }

            if (value > deleteEnd)
            {
                shifted = value - count;
            }

            return true;
        }

        private static TestCell CloneCell(TestCell source, string sheetName, int row, int column)
        {
            return new TestCell(new FormulaCellAddress(sheetName, row, column))
            {
                Formula = source.Formula,
                Expression = source.Expression,
                Value = source.Value
            };
        }
    }

    internal sealed class TestCell : IFormulaCell
    {
        public TestCell(FormulaCellAddress address)
        {
            Address = address;
            Value = FormulaValue.Blank;
        }

        public FormulaCellAddress Address { get; }

        public string? Formula { get; set; }

        public FormulaExpression? Expression { get; set; }

        public FormulaValue Value { get; set; }
    }

    internal class DictionaryValueResolver : IFormulaValueResolver
    {
        private readonly Dictionary<FormulaCellAddress, FormulaValue> _cells = new();
        private readonly Dictionary<string, FormulaValue> _names = new(StringComparer.OrdinalIgnoreCase);

        public void SetCell(FormulaCellAddress address, FormulaValue value)
        {
            _cells[address] = value;
        }

        public void SetName(string name, FormulaValue value)
        {
            _names[name] = value;
        }

        public bool TryResolveName(FormulaEvaluationContext context, string name, out FormulaValue value)
        {
            return _names.TryGetValue(name, out value);
        }

        public bool TryResolveReference(FormulaEvaluationContext context, FormulaReference reference, out FormulaValue value)
        {
            var origin = new FormulaCellAddress(context.Worksheet.Name, context.Address.Row, context.Address.Column);
            if (reference.Kind == FormulaReferenceKind.Cell)
            {
                if (!FormulaReferenceResolver.TryResolveCell(reference.Start, origin, out var address))
                {
                    value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
                    return true;
                }

                if (_cells.TryGetValue(address, out value))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(address.SheetName) &&
                    _cells.TryGetValue(address.WithSheet(null), out value))
                {
                    return true;
                }

                value = FormulaValue.Blank;
                return true;
            }

            if (!FormulaReferenceResolver.TryResolveRange(reference, origin, out var range))
            {
                value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
                return true;
            }

            var rows = range.End.Row - range.Start.Row + 1;
            var columns = range.End.Column - range.Start.Column + 1;
            var array = new FormulaArray(rows, columns, range.Start);
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var address = new FormulaCellAddress(range.Start.SheetName, range.Start.Row + row, range.Start.Column + column);
                    if (_cells.TryGetValue(address, out var cellValue))
                    {
                        array[row, column] = cellValue;
                    }
                    else
                    {
                        array[row, column] = FormulaValue.Blank;
                    }
                }
            }

            value = FormulaValue.FromArray(array);
            return true;
        }
    }

    internal sealed class StructuredReferenceResolver : IFormulaValueResolver, IFormulaStructuredReferenceResolver
    {
        private readonly DictionaryValueResolver _baseResolver = new();
        private readonly Dictionary<string, FormulaValue> _structured = new(StringComparer.OrdinalIgnoreCase);

        public void SetCell(FormulaCellAddress address, FormulaValue value)
        {
            _baseResolver.SetCell(address, value);
        }

        public void SetName(string name, FormulaValue value)
        {
            _baseResolver.SetName(name, value);
        }

        public void SetStructuredReference(FormulaStructuredReference reference, FormulaValue value)
        {
            _structured[CreateKey(reference)] = value;
        }

        public bool TryResolveName(FormulaEvaluationContext context, string name, out FormulaValue value)
        {
            return _baseResolver.TryResolveName(context, name, out value);
        }

        public bool TryResolveReference(FormulaEvaluationContext context, FormulaReference reference, out FormulaValue value)
        {
            return _baseResolver.TryResolveReference(context, reference, out value);
        }

        public bool TryResolveStructuredReference(
            FormulaEvaluationContext context,
            FormulaStructuredReference reference,
            out FormulaValue value)
        {
            return _structured.TryGetValue(CreateKey(reference), out value);
        }

        private static string CreateKey(FormulaStructuredReference reference)
        {
            var table = reference.TableName ?? string.Empty;
            var scope = reference.Scope.ToString();
            var column = reference.ColumnStart ?? string.Empty;
            var columnEnd = reference.ColumnEnd ?? string.Empty;
            var sheet = reference.Sheet?.ToString() ?? string.Empty;
            return $"{sheet}|{table}|{scope}|{column}|{columnEnd}";
        }
    }
}
