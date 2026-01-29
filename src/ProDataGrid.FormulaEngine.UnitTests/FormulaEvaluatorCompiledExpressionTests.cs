// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaEvaluatorCompiledExpressionTests
    {
        [Fact]
        public void Evaluate_Uses_CompiledExpression_Cache()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("1+2", new FormulaParseOptions());
            var registry = new ExcelFunctionRegistry();
            var telemetry = new FormulaCalculationTelemetry();
            workbook.Settings.CalculationObserver = telemetry;

            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                new FormulaCellAddress("Sheet1", 1, 1),
                registry);

            var evaluator = new FormulaEvaluator();
            var resolver = new DictionaryValueResolver();

            var first = evaluator.Evaluate(expression, context, resolver);
            var second = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, first.Kind);
            Assert.Equal(FormulaValueKind.Number, second.Kind);
            Assert.Equal(3, first.AsNumber());
            Assert.Equal(3, second.AsNumber());
            Assert.Equal(1, telemetry.CompiledExpressions);
            Assert.Equal(1, telemetry.CompileCacheHits);
        }

        [Fact]
        public void Evaluate_Skips_Compile_For_Union_Operators()
        {
            var workbook = new TestWorkbook("Book1");
            var sheet = workbook.GetWorksheet("Sheet1");
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("A1,B1", new FormulaParseOptions());
            var registry = new ExcelFunctionRegistry();
            var telemetry = new FormulaCalculationTelemetry();
            workbook.Settings.CalculationObserver = telemetry;

            var context = new FormulaEvaluationContext(
                workbook,
                sheet,
                new FormulaCellAddress("Sheet1", 1, 1),
                registry);

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(1));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 2), FormulaValue.FromNumber(2));

            var evaluator = new FormulaEvaluator();
            evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(0, telemetry.CompiledExpressions);
            Assert.Equal(0, telemetry.CompileCacheHits);
        }
    }
}
