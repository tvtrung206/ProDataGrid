// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Globalization;
using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaCalculationEngineTests
    {
        [Fact]
        public void CalculationEngine_Recalculates_Dependents()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            sheet.GetCell(1, 1).Value = FormulaValue.FromNumber(5);

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 2, "A1+1");
            engine.SetCellFormula(sheet, 1, 3, "B1+1");

            var result = engine.Recalculate(workbook, new[] { new FormulaCellAddress("Sheet1", 1, 1) });

            Assert.False(result.HasCycle);
            Assert.Equal(6, sheet.GetCell(1, 2).Value.AsNumber());
            Assert.Equal(7, sheet.GetCell(1, 3).Value.AsNumber());
        }

        [Fact]
        public void CalculationEngine_Marks_Cycle_With_Circ()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 1, "B1+1");
            engine.SetCellFormula(sheet, 1, 2, "A1+1");

            var result = engine.Recalculate(workbook, new[] { new FormulaCellAddress("Sheet1", 1, 1) });

            Assert.True(result.HasCycle);
            Assert.Equal(FormulaErrorType.Circ, sheet.GetCell(1, 1).Value.AsError().Type);
            Assert.Equal(FormulaErrorType.Circ, sheet.GetCell(1, 2).Value.AsError().Type);
        }

        [Fact]
        public void CalculationEngine_Spills_Array_Literal()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 1, "{1,2;3,4}");

            engine.Recalculate(workbook, new[] { new FormulaCellAddress("Sheet1", 1, 1) });

            var anchorValue = sheet.GetCell(1, 1).Value;
            Assert.Equal(FormulaValueKind.Array, anchorValue.Kind);

            var array = anchorValue.AsArray();
            Assert.Equal(1, array[0, 0].AsNumber());
            Assert.Equal(2, sheet.GetCell(1, 2).Value.AsNumber());
            Assert.Equal(3, sheet.GetCell(2, 1).Value.AsNumber());
            Assert.Equal(4, sheet.GetCell(2, 2).Value.AsNumber());
        }

        [Fact]
        public void CalculationEngine_Returns_Spill_Error_On_Conflict()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            sheet.GetCell(1, 2).Value = FormulaValue.FromNumber(99);

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 1, "{1,2}");

            engine.Recalculate(workbook, new[] { new FormulaCellAddress("Sheet1", 1, 1) });

            Assert.Equal(FormulaErrorType.Spill, sheet.GetCell(1, 1).Value.AsError().Type);
            Assert.Equal(99, sheet.GetCell(1, 2).Value.AsNumber());
        }

        [Fact]
        public void CalculationEngine_Iterates_Cycle_When_Enabled()
        {
            var workbook = new TestWorkbook("Book1");
            workbook.Settings.EnableIterativeCalculation = true;
            workbook.Settings.IterativeMaxIterations = 50;
            workbook.Settings.IterativeTolerance = 0.0001;

            var sheet = workbook.GetWorksheet("Sheet1");

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 1, "0.5*(A1+10)");

            engine.Recalculate(workbook, new[] { new FormulaCellAddress("Sheet1", 1, 1) });

            var value = sheet.GetCell(1, 1).Value.AsNumber();
            Assert.InRange(value, 9.9, 10.1);
        }

        [Fact]
        public void CalculationEngine_Updates_Dependencies_On_Name_Change()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            var initialRef = new FormulaReference(new FormulaReferenceAddress(FormulaReferenceMode.A1, 1, 1, false, false));
            workbook.Names.SetReference("Input", initialRef);

            engine.SetCellFormula(sheet, 1, 2, "Input+1");

            var formulaCell = new FormulaCellAddress("Sheet1", 1, 2);
            Assert.Contains(new FormulaCellAddress("Sheet1", 1, 1), engine.DependencyGraph.GetDependencies(formulaCell));

            using var subscription = engine.TrackNameChanges(workbook);
            var updatedRef = new FormulaReference(new FormulaReferenceAddress(FormulaReferenceMode.A1, 2, 1, false, false));
            workbook.Names.SetReference("Input", updatedRef);

            var updatedDependencies = engine.DependencyGraph.GetDependencies(formulaCell);
            Assert.Contains(new FormulaCellAddress("Sheet1", 2, 1), updatedDependencies);
            Assert.DoesNotContain(new FormulaCellAddress("Sheet1", 1, 1), updatedDependencies);
        }

        [Fact]
        public void CalculationEngine_Respects_Culture_Separators()
        {
            var workbook = new TestWorkbook("Book1");
            workbook.Settings.Culture = new CultureInfo("de-DE");
            var sheet = workbook.GetWorksheet("Sheet1");

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 1, "SUM(1;2)");
            engine.SetCellFormula(sheet, 1, 2, "1,5+1");

            engine.Recalculate(workbook, new[]
            {
                new FormulaCellAddress("Sheet1", 1, 1),
                new FormulaCellAddress("Sheet1", 1, 2)
            });

            Assert.Equal(3, sheet.GetCell(1, 1).Value.AsNumber());
            Assert.Equal(2.5, sheet.GetCell(1, 2).Value.AsNumber(), 6);
        }

        [Fact]
        public void CalculationEngine_ExternalReference_DoesNotTrigger_Circular_When_WorkbookDiffers()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");

            var external = new TestWorkbook("Book2");
            external.GetWorksheet("Sheet1").GetCell(1, 1).Value = FormulaValue.FromNumber(7);
            workbook.RegisterExternalWorkbook("Book2", external);

            var parser = new ExcelFormulaParser();
            var registry = new ExcelFunctionRegistry();
            var engine = new FormulaCalculationEngine(parser, registry);

            engine.SetCellFormula(sheet, 1, 1, "'[Book2]Sheet1'!A1");
            engine.Recalculate(workbook, new[] { new FormulaCellAddress("Sheet1", 1, 1) });

            Assert.Equal(7, sheet.GetCell(1, 1).Value.AsNumber());
        }
    }
}
