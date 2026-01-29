// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class ExcelFormulaFormatterTests
    {
        [Fact]
        public void Format_Includes_Leading_Equals_When_Configured()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("A1+1", new FormulaParseOptions());
            var formatter = new ExcelFormulaFormatter();

            var result = formatter.Format(expression, new FormulaFormatOptions
            {
                IncludeLeadingEquals = true
            });

            Assert.Equal("=A1+1", result);
        }

        [Fact]
        public void Format_Uses_Custom_Separators()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("SUM(1.5,2)", new FormulaParseOptions());
            var formatter = new ExcelFormulaFormatter();

            var result = formatter.Format(expression, new FormulaFormatOptions
            {
                ArgumentSeparator = ';',
                DecimalSeparator = ','
            });

            Assert.Equal("SUM(1,5;2)", result);
        }

        [Fact]
        public void Format_Structured_Reference_With_Scope()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("Table1[[#Headers],[Amount]]", new FormulaParseOptions());
            var formatter = new ExcelFormulaFormatter();

            var result = formatter.Format(expression, new FormulaFormatOptions());

            Assert.Equal("Table1[[#Headers],[Amount]]", result);
        }

        [Fact]
        public void Format_External_Sheet_Reference_With_Quotes()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("'[Book 1]Sheet 1'!A1", new FormulaParseOptions());
            var formatter = new ExcelFormulaFormatter();

            var result = formatter.Format(expression, new FormulaFormatOptions
            {
                IncludeLeadingEquals = true
            });

            Assert.Equal("='[Book 1]Sheet 1'!A1", result);
        }

        [Fact]
        public void Format_Array_Literal()
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse("{1,2;3,4}", new FormulaParseOptions());
            var formatter = new ExcelFormulaFormatter();

            var result = formatter.Format(expression, new FormulaFormatOptions());

            Assert.Equal("{1,2;3,4}", result);
        }
    }
}
