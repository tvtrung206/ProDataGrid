// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class WorkbookValueResolverTests
    {
        [Fact]
        public void Resolve_Cell_Formula_From_Workbook()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");

            var cellA1 = sheet.GetCell(1, 1);
            cellA1.Value = FormulaValue.FromNumber(5);

            var cellA2 = sheet.GetCell(2, 1);
            cellA2.Formula = "A1+1";

            var registry = new ExcelFunctionRegistry();
            var parser = new ExcelFormulaParser();
            var resolver = new WorkbookValueResolver(parser);
            var evaluator = new FormulaEvaluator();

            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                cellA2.Address,
                registry);

            var expression = parser.Parse(cellA2.Formula, new FormulaParseOptions());
            cellA2.Expression = expression;

            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(6, result.AsNumber());
        }

        [Fact]
        public void Resolve_Workbook_Name_Value()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            workbook.Names.SetValue("Rate", FormulaValue.FromNumber(2));

            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("Rate+1", new FormulaParseOptions());
            var resolver = new WorkbookValueResolver(parser);
            var evaluator = new FormulaEvaluator();
            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                new FormulaCellAddress("Sheet1", 1, 1),
                new ExcelFunctionRegistry());

            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(3, result.AsNumber());
        }

        [Fact]
        public void Resolve_Sheet_Name_Overrides_Workbook()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            workbook.Names.SetValue("Value", FormulaValue.FromNumber(10));
            ((TestWorksheet)sheet).Names.SetValue("Value", FormulaValue.FromNumber(2));

            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("Value+1", new FormulaParseOptions());
            var resolver = new WorkbookValueResolver(parser);
            var evaluator = new FormulaEvaluator();
            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                new FormulaCellAddress("Sheet1", 1, 1),
                new ExcelFunctionRegistry());

            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(3, result.AsNumber());
        }

        [Fact]
        public void Resolve_Name_Reference()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            sheet.GetCell(1, 1).Value = FormulaValue.FromNumber(5);

            var parser = new ExcelFormulaParser();
            workbook.Names.SetExpression("Input", parser.Parse("A1", new FormulaParseOptions()));

            var expression = parser.Parse("Input+1", new FormulaParseOptions());
            var resolver = new WorkbookValueResolver(parser);
            var evaluator = new FormulaEvaluator();
            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                new FormulaCellAddress("Sheet1", 1, 1),
                new ExcelFunctionRegistry());

            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(6, result.AsNumber());
        }

        [Fact]
        public void Resolve_Sheet_Range_Reference()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet1 = workbook.GetWorksheet("Sheet1");
            var sheet2 = workbook.AddWorksheet("Sheet2");
            var sheet3 = workbook.AddWorksheet("Sheet3");

            sheet1.GetCell(1, 1).Value = FormulaValue.FromNumber(1);
            sheet2.GetCell(1, 1).Value = FormulaValue.FromNumber(2);
            sheet3.GetCell(1, 1).Value = FormulaValue.FromNumber(3);

            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("SUM(Sheet1:Sheet3!A1)", new FormulaParseOptions());
            var resolver = new WorkbookValueResolver(parser);
            var evaluator = new FormulaEvaluator();
            var context = new FormulaEvaluationContext(
                workbook,
                sheet1,
                new FormulaCellAddress("Sheet1", 1, 1),
                new ExcelFunctionRegistry());

            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(6, result.AsNumber());
        }

        [Fact]
        public void Resolve_External_Reference()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");

            var external = new TestWorkbook("External");
            var externalSheet = external.GetWorksheet("Sheet1");
            externalSheet.GetCell(1, 1).Value = FormulaValue.FromNumber(7);
            workbook.RegisterExternalWorkbook("External", external);

            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("'[External]Sheet1'!A1", new FormulaParseOptions());
            var resolver = new WorkbookValueResolver(parser);
            var evaluator = new FormulaEvaluator();
            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                new FormulaCellAddress("Sheet1", 1, 1),
                new ExcelFunctionRegistry());

            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(7, result.AsNumber());
        }
    }
}
