// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using ProDataGrid.FormulaEngine;
using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class ExcelCompatibilityCorpusTests
    {
        [Fact]
        public void Evaluate_Compatibility_Corpus()
        {
            var corpus = LoadCorpus();
            foreach (var testCase in corpus.Cases)
            {
                var parser = new ExcelFormulaParser();
                var expression = parser.Parse(testCase.Formula, new FormulaParseOptions());

                var context = CreateContext(testCase.Culture, testCase.DateSystem);
                var resolver = new DictionaryValueResolver();
                ApplyCells(testCase, resolver);
                ApplyNames(testCase, resolver);

                var evaluator = new FormulaEvaluator();
                var result = evaluator.Evaluate(expression, context, resolver);
                try
                {
                    AssertExpected(testCase, result);
                }
                catch (Xunit.Sdk.XunitException ex)
                {
                    throw new Xunit.Sdk.XunitException($"Formula '{testCase.Formula}' failed: {ex.Message}");
                }
            }
        }

        private static ExcelCompatibilityCorpus LoadCorpus()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Compatibility", "ExcelCompatibilityCorpus.json");
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var corpus = JsonSerializer.Deserialize<ExcelCompatibilityCorpus>(json, options);
            if (corpus == null)
            {
                throw new InvalidOperationException("Failed to parse compatibility corpus.");
            }

            return corpus;
        }

        private static FormulaEvaluationContext CreateContext(string? cultureName, string? dateSystemName)
        {
            var workbook = new TestWorkbook("Book1");
            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                workbook.Settings.Culture = new CultureInfo(cultureName);
            }

            if (!string.IsNullOrWhiteSpace(dateSystemName) &&
                Enum.TryParse(dateSystemName, ignoreCase: true, out FormulaDateSystem parsed))
            {
                workbook.Settings.DateSystem = parsed;
            }

            var worksheet = workbook.GetWorksheet("Sheet1");
            var registry = new ExcelFunctionRegistry();
            var address = new FormulaCellAddress("Sheet1", 1, 1);
            return new FormulaEvaluationContext(workbook, worksheet, address, registry);
        }

        private static void ApplyCells(ExcelCompatibilityCase testCase, DictionaryValueResolver resolver)
        {
            if (testCase.Cells == null)
            {
                return;
            }

            foreach (var entry in testCase.Cells)
            {
                var address = ParseCellAddress(entry.Key);
                resolver.SetCell(address, ParseValue(entry.Value));
            }
        }

        private static void ApplyNames(ExcelCompatibilityCase testCase, DictionaryValueResolver resolver)
        {
            if (testCase.Names == null)
            {
                return;
            }

            foreach (var entry in testCase.Names)
            {
                resolver.SetName(entry.Key, ParseValue(entry.Value));
            }
        }

        private static void AssertExpected(ExcelCompatibilityCase testCase, FormulaValue result)
        {
            var expected = testCase.Result;
            var kind = (expected.Kind ?? string.Empty).Trim();

            switch (kind.ToUpperInvariant())
            {
                case "NUMBER":
                    Assert.Equal(FormulaValueKind.Number, result.Kind);
                    var expectedNumber = GetExpectedNumber(expected.Value);
                    var tolerance = expected.Tolerance ?? 0d;
                    if (tolerance == 0d)
                    {
                        Assert.Equal(expectedNumber, result.AsNumber());
                    }
                    else
                    {
                        Assert.InRange(result.AsNumber(), expectedNumber - tolerance, expectedNumber + tolerance);
                    }
                    break;
                case "TEXT":
                    Assert.Equal(FormulaValueKind.Text, result.Kind);
                    Assert.Equal(GetExpectedText(expected.Value), result.AsText());
                    break;
                case "BOOLEAN":
                    Assert.Equal(FormulaValueKind.Boolean, result.Kind);
                    Assert.Equal(GetExpectedBoolean(expected.Value), result.AsBoolean());
                    break;
                case "ERROR":
                    Assert.Equal(FormulaValueKind.Error, result.Kind);
                    var expectedError = ParseError(GetExpectedText(expected.Value));
                    Assert.Equal(expectedError, result.AsError().Type);
                    break;
                case "BLANK":
                    Assert.Equal(FormulaValueKind.Blank, result.Kind);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown expected kind '{kind}' for formula '{testCase.Formula}'.");
            }
        }

        private static double GetExpectedNumber(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDouble();
            }

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException("Expected numeric result value.");
        }

        private static bool GetExpectedBoolean(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.True)
            {
                return true;
            }

            if (value.ValueKind == JsonValueKind.False)
            {
                return false;
            }

            if (value.ValueKind == JsonValueKind.String &&
                bool.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException("Expected boolean result value.");
        }

        private static string GetExpectedText(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }

            throw new InvalidOperationException("Expected text result value.");
        }

        private static FormulaValue ParseValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    return FormulaValue.FromNumber(value.GetDouble());
                case JsonValueKind.String:
                    var text = value.GetString() ?? string.Empty;
                    if (TryParseError(text, out var errorType))
                    {
                        return FormulaValue.FromError(new FormulaError(errorType));
                    }
                    return FormulaValue.FromText(text);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return FormulaValue.FromBoolean(value.GetBoolean());
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return FormulaValue.Blank;
                default:
                    throw new InvalidOperationException("Unsupported JSON value for test cell.");
            }
        }

        private static FormulaCellAddress ParseCellAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new InvalidOperationException("Cell address is required.");
            }

            var sheetName = (string?)null;
            var reference = address;
            var bangIndex = address.LastIndexOf('!');
            if (bangIndex >= 0)
            {
                sheetName = address.Substring(0, bangIndex);
                reference = address.Substring(bangIndex + 1);

                if (sheetName.Length >= 2 && sheetName[0] == '\'' && sheetName[^1] == '\'')
                {
                    sheetName = sheetName.Substring(1, sheetName.Length - 2).Replace("''", "'");
                }
            }

            reference = reference.Replace("$", string.Empty);
            var index = 0;
            while (index < reference.Length && char.IsLetter(reference[index]))
            {
                index++;
            }

            if (index == 0 || index >= reference.Length)
            {
                throw new InvalidOperationException($"Invalid cell address '{address}'.");
            }

            var columnPart = reference.Substring(0, index).ToUpperInvariant();
            var rowPart = reference.Substring(index);
            if (!int.TryParse(rowPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var row))
            {
                throw new InvalidOperationException($"Invalid cell address '{address}'.");
            }

            var column = ColumnLettersToIndex(columnPart);
            return new FormulaCellAddress(sheetName, row, column);
        }

        private static int ColumnLettersToIndex(string letters)
        {
            var column = 0;
            for (var i = 0; i < letters.Length; i++)
            {
                var ch = letters[i];
                if (ch < 'A' || ch > 'Z')
                {
                    throw new InvalidOperationException($"Invalid column letters '{letters}'.");
                }

                column = (column * 26) + (ch - 'A' + 1);
            }

            return column;
        }

        private static bool TryParseError(string text, out FormulaErrorType type)
        {
            type = FormulaErrorType.Value;
            switch (text)
            {
                case "#DIV/0!":
                    type = FormulaErrorType.Div0;
                    return true;
                case "#N/A":
                    type = FormulaErrorType.NA;
                    return true;
                case "#NAME?":
                    type = FormulaErrorType.Name;
                    return true;
                case "#NULL!":
                    type = FormulaErrorType.Null;
                    return true;
                case "#NUM!":
                    type = FormulaErrorType.Num;
                    return true;
                case "#REF!":
                    type = FormulaErrorType.Ref;
                    return true;
                case "#VALUE!":
                    type = FormulaErrorType.Value;
                    return true;
                case "#CALC!":
                    type = FormulaErrorType.Calc;
                    return true;
                case "#SPILL!":
                    type = FormulaErrorType.Spill;
                    return true;
                case "#CIRC!":
                    type = FormulaErrorType.Circ;
                    return true;
                default:
                    return false;
            }
        }

        private static FormulaErrorType ParseError(string text)
        {
            if (TryParseError(text, out var type))
            {
                return type;
            }

            throw new InvalidOperationException($"Unknown error literal '{text}'.");
        }

        private sealed class ExcelCompatibilityCorpus
        {
            public List<ExcelCompatibilityCase> Cases { get; set; } = new();
        }

        private sealed class ExcelCompatibilityCase
        {
            public string Formula { get; set; } = string.Empty;

            public ExcelCompatibilityExpected Result { get; set; } = new();

            public Dictionary<string, JsonElement>? Cells { get; set; }

            public Dictionary<string, JsonElement>? Names { get; set; }

            public string? Culture { get; set; }

            public string? DateSystem { get; set; }
        }

        private sealed class ExcelCompatibilityExpected
        {
            public string? Kind { get; set; }

            public JsonElement Value { get; set; }

            public double? Tolerance { get; set; }
        }
    }
}
