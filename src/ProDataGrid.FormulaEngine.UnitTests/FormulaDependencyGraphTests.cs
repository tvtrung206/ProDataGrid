// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaDependencyGraphTests
    {
        [Fact]
        public void Graph_Tracks_Range_Dependencies()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("A1:B2+1", new FormulaParseOptions());
            var graph = new FormulaDependencyGraph();
            var cell = new FormulaCellAddress("Sheet1", 3, 3);

            graph.SetFormula(cell, expression);

            var dependencies = graph.GetDependencies(cell);
            Assert.Equal(4, dependencies.Count);
            Assert.Contains(new FormulaCellAddress("Sheet1", 1, 1), dependencies);
            Assert.Contains(new FormulaCellAddress("Sheet1", 1, 2), dependencies);
            Assert.Contains(new FormulaCellAddress("Sheet1", 2, 1), dependencies);
            Assert.Contains(new FormulaCellAddress("Sheet1", 2, 2), dependencies);

            var dependents = graph.GetDependents(new FormulaCellAddress("Sheet1", 1, 1));
            Assert.Contains(cell, dependents);
        }

        [Fact]
        public void Graph_Tracks_Name_Dependencies()
        {
            var parser = new ExcelFormulaParser();
            var graph = new FormulaDependencyGraph();
            var workbook = new TestWorkbook("Book1");
            var sheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var nameReference = new FormulaReference(new FormulaReferenceAddress(FormulaReferenceMode.A1, 1, 1, false, false));
            workbook.Names.SetReference("Input", nameReference);

            var expression = parser.Parse("Input+1", new FormulaParseOptions());
            var cell = new FormulaCellAddress("Sheet1", 3, 3);

            graph.SetFormula(cell, expression, sheet as IFormulaNameProvider, workbook as IFormulaNameProvider);

            var dependencies = graph.GetDependencies(cell);
            Assert.Contains(new FormulaCellAddress("Sheet1", 1, 1), dependencies);
        }

        [Fact]
        public void Graph_Uses_Sheet_Scoped_Names_First()
        {
            var parser = new ExcelFormulaParser();
            var graph = new FormulaDependencyGraph();
            var workbook = new TestWorkbook("Book1");
            var sheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");

            var workbookReference = new FormulaReference(new FormulaReferenceAddress(FormulaReferenceMode.A1, 1, 1, false, false));
            var sheetReference = new FormulaReference(new FormulaReferenceAddress(FormulaReferenceMode.A1, 2, 2, false, false));
            workbook.Names.SetReference("Input", workbookReference);
            sheet.Names.SetReference("Input", sheetReference);

            var expression = parser.Parse("Input+1", new FormulaParseOptions());
            var cell = new FormulaCellAddress("Sheet1", 3, 3);

            graph.SetFormula(cell, expression, sheet as IFormulaNameProvider, workbook as IFormulaNameProvider);

            var dependencies = graph.GetDependencies(cell);
            Assert.Contains(new FormulaCellAddress("Sheet1", 2, 2), dependencies);
            Assert.DoesNotContain(new FormulaCellAddress("Sheet1", 1, 1), dependencies);
        }

        [Fact]
        public void Graph_Indexes_Name_Dependents_By_Scope()
        {
            var parser = new ExcelFormulaParser();
            var graph = new FormulaDependencyGraph();
            var workbook = new TestWorkbook("Book1");
            var sheet = (TestWorksheet)workbook.GetWorksheet("Sheet1");
            var nameReference = new FormulaReference(new FormulaReferenceAddress(FormulaReferenceMode.A1, 1, 1, false, false));
            workbook.Names.SetReference("Rate", nameReference);

            var expression = parser.Parse("Rate+1", new FormulaParseOptions());
            var cell = new FormulaCellAddress("Sheet1", 2, 2);

            graph.SetFormula(cell, expression, sheet as IFormulaNameProvider, workbook as IFormulaNameProvider);

            var workbookDependents = graph.GetFormulaCellsForName("Rate", null, false);
            Assert.Contains(cell, workbookDependents);

            var sheetDependents = graph.GetFormulaCellsForName("Rate", "Sheet1", true);
            Assert.DoesNotContain(cell, sheetDependents);
        }

        [Fact]
        public void Graph_Returns_Recalculation_Order()
        {
            var parser = new ExcelFormulaParser();
            var graph = new FormulaDependencyGraph();
            var options = new FormulaParseOptions();

            var b1 = new FormulaCellAddress("Sheet1", 1, 2);
            var c1 = new FormulaCellAddress("Sheet1", 1, 3);

            graph.SetFormula(b1, parser.Parse("A1+1", options));
            graph.SetFormula(c1, parser.Parse("B1+1", options));

            var dirty = new[] { new FormulaCellAddress("Sheet1", 1, 1) };
            var success = graph.TryGetRecalculationOrder(dirty, out var order, out var cycle);

            Assert.True(success);
            Assert.Empty(cycle);
            Assert.Equal(new[] { b1, c1 }, order);
        }

        [Fact]
        public void Graph_Detects_Cycles()
        {
            var parser = new ExcelFormulaParser();
            var graph = new FormulaDependencyGraph();
            var options = new FormulaParseOptions();

            var a1 = new FormulaCellAddress("Sheet1", 1, 1);
            var b1 = new FormulaCellAddress("Sheet1", 1, 2);

            graph.SetFormula(a1, parser.Parse("B1+1", options));
            graph.SetFormula(b1, parser.Parse("A1+1", options));

            var dirty = new[] { a1 };
            var success = graph.TryGetRecalculationOrder(dirty, out var order, out var cycle);

            Assert.False(success);
            Assert.Empty(order);
            Assert.Contains(a1, cycle);
            Assert.Contains(b1, cycle);
        }

        [Fact]
        public void Graph_Ignores_External_References()
        {
            var parser = new ExcelFormulaParser();
            var graph = new FormulaDependencyGraph();
            var workbook = new TestWorkbook("Book1");
            var external = new TestWorkbook("Book2");
            workbook.RegisterExternalWorkbook("Book2", external);

            var expression = parser.Parse("'[Book2]Sheet1'!A1", new FormulaParseOptions());
            var cell = new FormulaCellAddress("Sheet1", 1, 1);

            graph.SetFormula(cell, expression, workbook, null, null);

            var dependencies = graph.GetDependencies(cell);
            Assert.Empty(dependencies);
        }
    }
}
