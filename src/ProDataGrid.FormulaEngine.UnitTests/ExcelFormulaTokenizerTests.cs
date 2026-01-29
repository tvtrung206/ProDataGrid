// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class ExcelFormulaTokenizerTests
    {
        [Fact]
        public void Tokenize_Parses_Error_Literal()
        {
            var tokenizer = new ExcelFormulaTokenizer(new FormulaParseOptions());
            var tokens = tokenizer.Tokenize("=#DIV/0!");

            Assert.Equal(FormulaTokenType.Error, tokens[0].Type);
            Assert.Equal("#DIV/0!", tokens[0].Text);
        }

        [Fact]
        public void Tokenize_Parses_Sheet_Prefix()
        {
            var tokenizer = new ExcelFormulaTokenizer(new FormulaParseOptions());
            var tokens = tokenizer.Tokenize("='My Sheet'!A1");

            Assert.Equal(FormulaTokenType.Name, tokens[0].Type);
            Assert.Equal("My Sheet", tokens[0].Text);
            Assert.Equal(FormulaTokenType.Exclamation, tokens[1].Type);
            Assert.Equal(FormulaTokenType.Name, tokens[2].Type);
            Assert.Equal("A1", tokens[2].Text);
        }

        [Fact]
        public void Tokenize_Parses_Array_Literal_Braces()
        {
            var tokenizer = new ExcelFormulaTokenizer(new FormulaParseOptions());
            var tokens = tokenizer.Tokenize("={1,2;3,4}");

            Assert.Equal(FormulaTokenType.OpenBrace, tokens[0].Type);
            Assert.Equal(FormulaTokenType.Number, tokens[1].Type);
            Assert.Equal(FormulaTokenType.Comma, tokens[2].Type);
            Assert.Equal(FormulaTokenType.Number, tokens[3].Type);
            Assert.Equal(FormulaTokenType.Semicolon, tokens[4].Type);
            Assert.Equal(FormulaTokenType.Number, tokens[5].Type);
            Assert.Equal(FormulaTokenType.Comma, tokens[6].Type);
            Assert.Equal(FormulaTokenType.Number, tokens[7].Type);
            Assert.Equal(FormulaTokenType.CloseBrace, tokens[8].Type);
        }

        [Fact]
        public void Tokenize_Parses_Percent_Operator()
        {
            var tokenizer = new ExcelFormulaTokenizer(new FormulaParseOptions());
            var tokens = tokenizer.Tokenize("10%");

            Assert.Equal(FormulaTokenType.Number, tokens[0].Type);
            Assert.Equal(FormulaTokenType.Operator, tokens[1].Type);
            Assert.Equal("%", tokens[1].Text);
        }

        [Fact]
        public void Tokenize_Parses_Intersection_Operator()
        {
            var tokenizer = new ExcelFormulaTokenizer(new FormulaParseOptions());
            var tokens = tokenizer.Tokenize("A1 B2");

            Assert.Equal(FormulaTokenType.Name, tokens[0].Type);
            Assert.Equal(FormulaTokenType.Intersection, tokens[1].Type);
            Assert.Equal(FormulaTokenType.Name, tokens[2].Type);
        }

        [Fact]
        public void Tokenize_Respects_Culture_Separators()
        {
            var options = new FormulaParseOptions
            {
                ArgumentSeparator = ';',
                DecimalSeparator = ','
            };
            var tokenizer = new ExcelFormulaTokenizer(options);
            var tokens = tokenizer.Tokenize("SUM(1,5;2)");

            Assert.Equal(FormulaTokenType.Name, tokens[0].Type);
            Assert.Equal(FormulaTokenType.OpenParen, tokens[1].Type);
            Assert.Equal(FormulaTokenType.Number, tokens[2].Type);
            Assert.Equal("1,5", tokens[2].Text);
            Assert.Equal(FormulaTokenType.Semicolon, tokens[3].Type);
            Assert.Equal(FormulaTokenType.Number, tokens[4].Type);
            Assert.Equal("2", tokens[4].Text);
            Assert.Equal(FormulaTokenType.CloseParen, tokens[5].Type);
        }
    }
}
