// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;
using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaEvaluatorTests
    {
        [Fact]
        public void Evaluate_Respects_Precedence()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("1+2*3", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(7, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Resolves_Cell_Reference()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("A1+1", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(5));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(6, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Array_Addition()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("A1:B2+1", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(1));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 2), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(3));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 2), FormulaValue.FromNumber(4));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            var array = result.AsArray();
            Assert.Equal(2, array.RowCount);
            Assert.Equal(2, array.ColumnCount);
            Assert.Equal(2, array[0, 0].AsNumber());
            Assert.Equal(3, array[0, 1].AsNumber());
            Assert.Equal(4, array[1, 0].AsNumber());
            Assert.Equal(5, array[1, 1].AsNumber());
        }

        [Fact]
        public void Evaluate_ImplicitIntersection_In_If()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("IF(A1:B2,1,0)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(0));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 2), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 2), FormulaValue.FromNumber(5));

            var context = CreateContext(row: 2, column: 2);
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Sum_Range()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("SUM(A1:B2)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(1));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 2), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(3));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 2), FormulaValue.FromNumber(4));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(10, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Sum_Uses_Direct_Arguments_For_Text()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("SUM(\"2\",TRUE)", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(3, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Average_Ignores_Text_In_Range()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("AVERAGE(A1:A3)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("hello"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromNumber(4));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(3, result.AsNumber());
        }

        [Fact]
        public void Evaluate_And_Ignores_Text_In_Range()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("AND(A1:A3)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromBoolean(true));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("hello"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromNumber(1));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Boolean, result.Kind);
            Assert.True(result.AsBoolean());
        }

        [Fact]
        public void Evaluate_Or_Ignores_Text_In_Range()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("OR(A1:A3)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromText("hello"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(0));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromBoolean(false));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Boolean, result.Kind);
            Assert.False(result.AsBoolean());
        }

        [Fact]
        public void Evaluate_And_Propagates_Error()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("AND(TRUE,#DIV/0!)", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Error, result.Kind);
            Assert.Equal(FormulaErrorType.Div0, result.AsError().Type);
        }

        [Fact]
        public void Evaluate_Or_Propagates_Error()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("OR(FALSE,#VALUE!)", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Error, result.Kind);
            Assert.Equal(FormulaErrorType.Value, result.AsError().Type);
        }

        [Fact]
        public void Evaluate_Comparison_Text_And_Number_Uses_Text_Comparison()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("\"a\">1", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Boolean, result.Kind);
            Assert.True(result.AsBoolean());
        }

        [Fact]
        public void Evaluate_Count_Uses_Direct_Arguments()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNT(1,\"2\",TRUE,\"text\")", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(3, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Count_Ignores_Text_In_Range()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNT(A1:A3)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(1));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("2"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromBoolean(true));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Counta_Ignores_Blanks()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTA(A1:A3)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(1));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("hello"));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(2, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Percent_Operator()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("10%", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(0.1, result.AsNumber(), 6);
        }

        [Fact]
        public void Evaluate_Array_Literal_In_Sum()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("SUM({1,2;3,4})", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(10, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Union_Skips_Missing_Cells()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIF((A1,C1),\"\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.Blank);
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 3), FormulaValue.Blank);

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(2, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Intersection_Returns_Null_Error_When_Empty()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("(A1:B1 C1:D1)", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, new DictionaryValueResolver());

            Assert.Equal(FormulaValueKind.Error, result.Kind);
            Assert.Equal(FormulaErrorType.Null, result.AsError().Type);
        }

        [Fact]
        public void Evaluate_Intersection_In_Sum()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("SUM((A1:B2 B2:C3))", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 2), FormulaValue.FromNumber(5));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(5, result.AsNumber());
        }

        [Fact]
        public void Evaluate_Structured_Reference()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("Table1[Amount]", new FormulaParseOptions());

            var resolver = new StructuredReferenceResolver();
            resolver.SetStructuredReference(
                new FormulaStructuredReference(null, "Table1", FormulaStructuredReferenceScope.None, "Amount", null),
                FormulaValue.FromNumber(42));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(42, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIf_Matches_Numeric_Criteria()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIF(A1:A3,2)", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("2"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromNumber(3));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(2, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIf_Counts_Blanks()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIF(A1:A3,\"\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText(string.Empty));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromNumber(1));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(2, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIf_Escaped_Asterisk_Is_Literal()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIF(A1:A4,\"A~*B\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromText("A*B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("AB"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromText("A?B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 4, 1), FormulaValue.FromText("A~B"));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIf_Escaped_Question_Is_Literal()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIF(A1:A3,\"A~?B\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromText("A?B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("A1B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromText("A*B"));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIf_Escaped_Tilde_Is_Literal()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIF(A1:A3,\"A~~B\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromText("A~B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("A~~B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromText("AB"));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIfs_Escaped_Wildcard_With_Additional_Criteria()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIFS(A1:A3,\"A~*B\",B1:B3,\">1\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromText("A*B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromText("A*B"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromText("AB"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 2), FormulaValue.FromNumber(1));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 2), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 2), FormulaValue.FromNumber(3));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Fact]
        public void Evaluate_CountIfs_Matches_All_Criteria()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("COUNTIFS(A1:A3,2,B1:B3,\"x\")", new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(2));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 1), FormulaValue.FromNumber(3));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 2), FormulaValue.FromText("x"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 2), FormulaValue.FromText("y"));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 3, 2), FormulaValue.FromText("x"));

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Number, result.Kind);
            Assert.Equal(1, result.AsNumber());
        }

        [Theory]
        [MemberData(nameof(IsParityCases))]
        public void Evaluate_Is_Functions_Parity(string formula, FormulaValue? cellValue, bool expected)
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse(formula, new FormulaParseOptions());

            var resolver = new DictionaryValueResolver();
            if (cellValue.HasValue)
            {
                resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), cellValue.Value);
            }

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(FormulaValueKind.Boolean, result.Kind);
            Assert.Equal(expected, result.AsBoolean());
        }

        [Fact]
        public void Evaluate_IsLogical_And_IsText()
        {
            var parser = new ExcelFormulaParser();
            var logicalExpression = parser.Parse("ISLOGICAL(TRUE)", new FormulaParseOptions());
            var textExpression = parser.Parse("ISTEXT(\"hello\")", new FormulaParseOptions());

            var context = CreateContext();
            var evaluator = new FormulaEvaluator();

            var logicalResult = evaluator.Evaluate(logicalExpression, context, new DictionaryValueResolver());
            var textResult = evaluator.Evaluate(textExpression, context, new DictionaryValueResolver());

            Assert.True(logicalResult.AsBoolean());
            Assert.True(textResult.AsBoolean());
        }

        [Fact]
        public void Evaluate_R1C1_Relative_Reference()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("R[1]C", new FormulaParseOptions { ReferenceMode = FormulaReferenceMode.R1C1 });

            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(7));

            var context = CreateContext(row: 1, column: 1);
            var evaluator = new FormulaEvaluator();
            var result = evaluator.Evaluate(expression, context, resolver);

            Assert.Equal(7, result.AsNumber());
        }

        private static FormulaEvaluationContext CreateContext(int row = 1, int column = 1)
        {
            var workbook = new TestWorkbook("Book1");
            var worksheet = workbook.GetWorksheet("Sheet1");
            var registry = new ExcelFunctionRegistry();
            var address = new FormulaCellAddress("Sheet1", row, column);
            return new FormulaEvaluationContext(workbook, worksheet, address, registry);
        }

        public static IEnumerable<object?[]> IsParityCases => new List<object?[]>
        {
            new object?[] { "ISNUMBER(1)", null, true },
            new object?[] { "ISNUMBER(\"1\")", null, false },
            new object?[] { "ISNUMBER(TRUE)", null, false },
            new object?[] { "ISNUMBER(#DIV/0!)", null, false },
            new object?[] { "ISNUMBER(A1)", FormulaValue.FromNumber(2), true },
            new object?[] { "ISNUMBER(A1)", FormulaValue.FromText("2"), false },
            new object?[] { "ISTEXT(\"hello\")", null, true },
            new object?[] { "ISTEXT(1)", null, false },
            new object?[] { "ISTEXT(\"\")", null, true },
            new object?[] { "ISTEXT(A1)", FormulaValue.FromText("text"), true },
            new object?[] { "ISLOGICAL(TRUE)", null, true },
            new object?[] { "ISLOGICAL(FALSE)", null, true },
            new object?[] { "ISLOGICAL(0)", null, false },
            new object?[] { "ISLOGICAL(A1)", FormulaValue.FromBoolean(true), true },
            new object?[] { "ISBLANK(A1)", FormulaValue.Blank, true },
            new object?[] { "ISBLANK(A1)", FormulaValue.FromText(string.Empty), false },
            new object?[] { "ISBLANK(\"\")", null, false },
            new object?[] { "ISBLANK(1)", null, false },
            new object?[] { "ISBLANK(#N/A)", null, false }
        };
    }
}
