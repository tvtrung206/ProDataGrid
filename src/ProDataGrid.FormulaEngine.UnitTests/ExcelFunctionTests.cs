// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using ProDataGrid.FormulaEngine.Excel;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class ExcelFunctionTests
    {
        [Fact]
        public void Evaluate_Rounding_And_Mod_Functions()
        {
            Assert.Equal(5, Evaluate("ABS(-5)").AsNumber());
            Assert.Equal(1.24, Evaluate("ROUND(1.235,2)").AsNumber(), 2);
            Assert.Equal(1200, Evaluate("ROUND(1234,-2)").AsNumber());
            Assert.Equal(-1.3, Evaluate("ROUNDUP(-1.21,1)").AsNumber(), 2);
            Assert.Equal(-1.2, Evaluate("ROUNDDOWN(-1.29,1)").AsNumber(), 2);
            Assert.Equal(1, Evaluate("MOD(-3,2)").AsNumber());
            Assert.Equal(-1, Evaluate("MOD(3,-2)").AsNumber());
        }

        [Fact]
        public void Evaluate_If_Error_Functions()
        {
            Assert.Equal(1, Evaluate("IF(TRUE,1,1/0)").AsNumber());
            Assert.Equal(2, Evaluate("IF(FALSE,1/0,2)").AsNumber());
            Assert.Equal(5, Evaluate("IFERROR(1/0,5)").AsNumber());
            Assert.Equal(7, Evaluate("IFNA(#N/A,7)").AsNumber());
        }

        [Fact]
        public void Evaluate_Logical_Info_Functions()
        {
            Assert.Equal(FormulaValueKind.Boolean, Evaluate("NOT(0)").Kind);
            Assert.True(Evaluate("NOT(0)").AsBoolean());
            Assert.False(Evaluate("ISERROR(1)").AsBoolean());
            Assert.True(Evaluate("ISERROR(1/0)").AsBoolean());
            Assert.True(Evaluate("ISNA(#N/A)").AsBoolean());
            Assert.False(Evaluate("ISNA(#DIV/0!)").AsBoolean());
            Assert.True(Evaluate("ISNUMBER(1)").AsBoolean());
            Assert.False(Evaluate("ISNUMBER(\"1\")").AsBoolean());
            Assert.True(Evaluate("ISTEXT(\"hello\")").AsBoolean());
        }

        [Fact]
        public void Evaluate_Text_Functions()
        {
            Assert.Equal("He", Evaluate("LEFT(\"Hello\",2)").AsText());
            Assert.Equal("lo", Evaluate("RIGHT(\"Hello\",2)").AsText());
            Assert.Equal("ell", Evaluate("MID(\"Hello\",2,3)").AsText());
            Assert.Equal(5, Evaluate("LEN(\"Hello\")").AsNumber());
            Assert.Equal("AABB", Evaluate("CONCAT(\"AA\",\"BB\")").AsText());
            Assert.Equal("a b", Evaluate("TRIM(\"  a   b  \")").AsText());
            Assert.Equal("abc", Evaluate("LOWER(\"AbC\")").AsText());
            Assert.Equal("ABC", Evaluate("UPPER(\"AbC\")").AsText());
        }

        [Fact]
        public void Evaluate_Dynamic_Array_Functions()
        {
            var sequence = Evaluate("SEQUENCE(2,3,1,1)").AsArray();
            Assert.Equal(1, sequence[0, 0].AsNumber());
            Assert.Equal(2, sequence[0, 1].AsNumber());
            Assert.Equal(3, sequence[0, 2].AsNumber());
            Assert.Equal(4, sequence[1, 0].AsNumber());
            Assert.Equal(5, sequence[1, 1].AsNumber());
            Assert.Equal(6, sequence[1, 2].AsNumber());

            var filtered = Evaluate("FILTER({1;2;3},{TRUE;FALSE;TRUE})").AsArray();
            Assert.Equal(2, filtered.RowCount);
            Assert.Equal(1, filtered.ColumnCount);
            Assert.Equal(1, filtered[0, 0].AsNumber());
            Assert.Equal(3, filtered[1, 0].AsNumber());

            var sorted = Evaluate("SORT({3;1;2})").AsArray();
            Assert.Equal(1, sorted[0, 0].AsNumber());
            Assert.Equal(2, sorted[1, 0].AsNumber());
            Assert.Equal(3, sorted[2, 0].AsNumber());

            var unique = Evaluate("UNIQUE({1;2;2;3})").AsArray();
            Assert.Equal(3, unique.RowCount);
            Assert.Equal(1, unique.ColumnCount);
            Assert.Equal(1, unique[0, 0].AsNumber());
            Assert.Equal(2, unique[1, 0].AsNumber());
            Assert.Equal(3, unique[2, 0].AsNumber());
        }

        [Fact]
        public void Evaluate_TextJoin_TextSplit_Functions()
        {
            Assert.Equal("a|b", Evaluate("TEXTJOIN(\"|\",TRUE,{\"a\";\"\";\"b\"})").AsText());

            var split = Evaluate("TEXTSPLIT(\"a,b;c,d\",\",\",\";\")").AsArray();
            Assert.Equal(2, split.RowCount);
            Assert.Equal(2, split.ColumnCount);
            Assert.Equal("a", split[0, 0].AsText());
            Assert.Equal("b", split[0, 1].AsText());
            Assert.Equal("c", split[1, 0].AsText());
            Assert.Equal("d", split[1, 1].AsText());
        }

        [Fact]
        public void Evaluate_Lookup_Functions()
        {
            Assert.Equal(4, Evaluate("INDEX({1,2;3,4},2,2)").AsNumber());
            Assert.Equal(2, Evaluate("INDEX({1,2,3},2)").AsNumber());
            Assert.Equal(3, Evaluate("MATCH(3,{1,2,3},0)").AsNumber());
            Assert.Equal(3, Evaluate("MATCH(5,{1,2,4,6},1)").AsNumber());
            Assert.Equal(2, Evaluate("MATCH(5,{6,5,4,2},-1)").AsNumber());
            Assert.Equal("b", Evaluate("VLOOKUP(2,{1,\"a\";2,\"b\";3,\"c\"},2,FALSE)").AsText());
            Assert.Equal("b", Evaluate("VLOOKUP(2.5,{1,\"a\";2,\"b\";3,\"c\"},2,TRUE)").AsText());
            Assert.Equal(2, Evaluate("HLOOKUP(\"B\",{\"A\",\"B\";1,2},2,FALSE)").AsNumber());
            Assert.Equal(2, Evaluate("XMATCH(2,{1;2;3})").AsNumber());
            Assert.Equal(20, Evaluate("XLOOKUP(2,{1;2;3},{10;20;30})").AsNumber());
            Assert.Equal("NF", Evaluate("XLOOKUP(\"c\",{\"a\";\"b\"},{1;2},\"NF\")").AsText());
            Assert.Equal(30, Evaluate("XLOOKUP(2,{1;2;2;3},{10;20;30;40},\"NF\",0,-1)").AsNumber());
            Assert.Equal(2, Evaluate("XLOOKUP(\"b*\",{\"aa\";\"bb\";\"bc\"},{1;2;3},\"NF\",2)").AsNumber());
        }

        [Fact]
        public void Evaluate_Offset_Indirect_Random_Functions()
        {
            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 1, 1), FormulaValue.FromNumber(10));
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(20));

            Assert.Equal(20, Evaluate("OFFSET(A1,1,0)", resolver).AsNumber());
            Assert.Equal(10, Evaluate("INDIRECT(\"A1\")", resolver).AsNumber());
            Assert.Equal(20, Evaluate("INDIRECT(\"R2C1\",FALSE)", resolver).AsNumber());

            var rand = Evaluate("RAND()").AsNumber();
            Assert.InRange(rand, 0d, 1d);

            var between = Evaluate("RANDBETWEEN(1,3)").AsNumber();
            Assert.InRange(between, 1d, 3d);
        }

        [Fact]
        public void Evaluate_IsBlank_Function()
        {
            var resolver = new DictionaryValueResolver();
            resolver.SetCell(new FormulaCellAddress("Sheet1", 2, 1), FormulaValue.FromNumber(3));

            Assert.True(Evaluate("ISBLANK(A1)", resolver).AsBoolean());
            Assert.False(Evaluate("ISBLANK(A2)", resolver).AsBoolean());
        }

        [Fact]
        public void Evaluate_Date_Time_Functions()
        {
            var expectedSerial = ToSerial(new DateTime(2024, 1, 2));
            Assert.Equal(expectedSerial, Evaluate("DATE(2024,1,2)").AsNumber(), 6);
            Assert.Equal(60, Evaluate("DATE(1900,2,29)").AsNumber());
            Assert.Equal(0.0625, Evaluate("TIME(1,30,0)").AsNumber(), 6);

            Assert.Equal(1900, Evaluate("YEAR(60)").AsNumber());
            Assert.Equal(2, Evaluate("MONTH(60)").AsNumber());
            Assert.Equal(29, Evaluate("DAY(60)").AsNumber());

            Assert.Equal(13, Evaluate("HOUR(TIME(13,45,30))").AsNumber());
            Assert.Equal(45, Evaluate("MINUTE(TIME(13,45,30))").AsNumber());
            Assert.Equal(30, Evaluate("SECOND(TIME(13,45,30))").AsNumber());

            Assert.Equal(expectedSerial, Evaluate("DATEVALUE(\"2024-01-02\")").AsNumber(), 6);
            Assert.Equal(0.573264, Evaluate("TIMEVALUE(\"13:45:30\")").AsNumber(), 6);
        }

        [Fact]
        public void Evaluate_EoMonth_Workday_Networkdays()
        {
            var expectedEomonth = ToSerial(new DateTime(2024, 2, 29));
            Assert.Equal(expectedEomonth, Evaluate("EOMONTH(DATE(2024,1,15),1)").AsNumber(), 6);

            var expectedWorkday = ToSerial(new DateTime(2024, 1, 5));
            Assert.Equal(expectedWorkday, Evaluate("WORKDAY(DATE(2024,1,1),4)").AsNumber(), 6);

            Assert.Equal(5, Evaluate("NETWORKDAYS(DATE(2024,1,1),DATE(2024,1,5))").AsNumber());
            Assert.Equal(4, Evaluate("NETWORKDAYS(DATE(2024,1,1),DATE(2024,1,5),DATE(2024,1,2))").AsNumber());
        }

        [Fact]
        public void Evaluate_DateValue_Uses_Culture()
        {
            var expectedSerial = ToSerial(new DateTime(2024, 1, 31));
            Assert.Equal(expectedSerial, Evaluate("DATEVALUE(\"31/01/2024\")", culture: new System.Globalization.CultureInfo("en-GB")).AsNumber(), 6);
        }

        private static FormulaValue Evaluate(
            string formula,
            DictionaryValueResolver? resolver = null,
            System.Globalization.CultureInfo? culture = null)
        {
            var parser = new ExcelFormulaParser();
            var expression = parser.Parse(formula, new FormulaParseOptions());

            var context = CreateContext(culture);
            var evaluator = new FormulaEvaluator();
            return evaluator.Evaluate(expression, context, resolver ?? new DictionaryValueResolver());
        }

        private static FormulaEvaluationContext CreateContext(System.Globalization.CultureInfo? culture = null)
        {
            var workbook = new TestWorkbook("Book1");
            var worksheet = workbook.GetWorksheet("Sheet1");
            var registry = new ExcelFunctionRegistry();
            var address = new FormulaCellAddress("Sheet1", 1, 1);
            if (culture != null)
            {
                workbook.Settings.Culture = culture;
            }
            return new FormulaEvaluationContext(workbook, worksheet, address, registry);
        }

        private static double ToSerial(DateTime date)
        {
            var epoch = new DateTime(1899, 12, 31);
            var serial = (date - epoch).TotalDays;
            if (date >= new DateTime(1900, 3, 1))
            {
                serial += 1;
            }

            return serial;
        }
    }
}
