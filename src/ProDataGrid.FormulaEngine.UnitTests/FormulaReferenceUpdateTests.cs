// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaReferenceUpdateTests
    {
        [Fact]
        public void InsertRows_ShiftsA1References()
        {
            var workbook = new TestWorkbook("Book");
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(worksheet, 1, 1, "A2");
            worksheet.InsertRows(2, 1);
            engine.InsertRows(workbook, "Sheet1", 2, 1, formatter);

            var cell = worksheet.GetCell(1, 1);
            Assert.Equal("A3", cell.Formula);
        }

        [Fact]
        public void DeleteRows_ReferenceDeletedBecomesRef()
        {
            var workbook = new TestWorkbook("Book");
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(worksheet, 1, 1, "A2");
            worksheet.DeleteRows(2, 1);
            engine.DeleteRows(workbook, "Sheet1", 2, 1, formatter);

            var cell = worksheet.GetCell(1, 1);
            Assert.Equal("#REF!", cell.Formula);
        }

        [Fact]
        public void DeleteRows_ShrinksRanges()
        {
            var workbook = new TestWorkbook("Book");
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(worksheet, 1, 1, "SUM(A1:A5)");
            worksheet.DeleteRows(2, 2);
            engine.DeleteRows(workbook, "Sheet1", 2, 2, formatter);

            var cell = worksheet.GetCell(1, 1);
            Assert.Equal("SUM(A1:A3)", cell.Formula);
        }

        [Fact]
        public void InsertRows_ShiftsR1C1RelativeReferences()
        {
            var workbook = new TestWorkbook("Book");
            workbook.Settings.ReferenceMode = FormulaReferenceMode.R1C1;
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(worksheet, 5, 2, "R[1]C");
            worksheet.InsertRows(6, 1);
            engine.InsertRows(workbook, "Sheet1", 6, 1, formatter);

            var cell = worksheet.GetCell(5, 2);
            Assert.Equal("R[2]C", cell.Formula);
        }

        [Fact]
        public void RenameSheet_UpdatesReferences()
        {
            var workbook = new TestWorkbook("Book");
            var sheet2 = (TestWorksheet)workbook.AddWorksheet("Sheet2");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(sheet2, 1, 1, "Sheet1!A1");
            workbook.RenameWorksheet("Sheet1", "Summary");
            engine.RenameSheet(workbook, "Sheet1", "Summary", formatter);

            var cell = sheet2.GetCell(1, 1);
            Assert.Equal("Summary!A1", cell.Formula);
        }

        [Fact]
        public void RenameTable_UpdatesStructuredReferences()
        {
            var workbook = new TestWorkbook("Book");
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(worksheet, 1, 1, "SalesTable[Amount]");
            engine.RenameTable(workbook, "SalesTable", "RevenueTable", formatter);

            var cell = worksheet.GetCell(1, 1);
            Assert.Equal("RevenueTable[Amount]", cell.Formula);
        }

        [Fact]
        public void RenameTableColumn_UpdatesStructuredReferences()
        {
            var workbook = new TestWorkbook("Book");
            var worksheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var engine = new FormulaCalculationEngine(new ExcelFormulaParser(), new ExcelFunctionRegistry());
            var formatter = new ExcelFormulaFormatter();

            engine.SetCellFormula(worksheet, 1, 1, "SalesTable[Amount]");
            engine.RenameTableColumn(workbook, "SalesTable", "Amount", "Total", formatter);

            var cell = worksheet.GetCell(1, 1);
            Assert.Equal("SalesTable[Total]", cell.Formula);
        }
    }
}
