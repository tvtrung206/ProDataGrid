// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;
using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaEngineStressTests
    {
        [Fact]
        public void Recalculate_LargeGrid_Completes()
        {
            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            var workbook = new TestWorkbook("Book1");
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var formulaCells = new List<FormulaCellAddress>();

            for (var row = 1; row <= 1000; row++)
            {
                var valueCell = worksheet.GetCell(row, 1);
                valueCell.Value = FormulaValue.FromNumber(row);
                engine.SetCellFormula(worksheet, row, 2, $"=A{row}*2");
                formulaCells.Add(new FormulaCellAddress("Sheet1", row, 2));
            }

            var result = engine.Recalculate(workbook, formulaCells);
            Assert.Equal(formulaCells.Count, result.Recalculated.Count);

            var dirtyAddress = new FormulaCellAddress("Sheet1", 500, 1);
            var dirtyCell = worksheet.GetCell(500, 1);
            dirtyCell.Value = FormulaValue.FromNumber(1234);

            engine.RecalculateIfAutomatic(workbook, new[] { dirtyAddress });

            var outputCell = worksheet.GetCell(500, 2);
            Assert.Equal(FormulaValueKind.Number, outputCell.Value.Kind);
            Assert.Equal(2468d, outputCell.Value.AsNumber());
        }
    }
}
