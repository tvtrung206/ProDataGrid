// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaAddressTests
    {
        [Fact]
        public void FormulaCellAddress_Equality_Ignores_Sheet_Case()
        {
            var left = new FormulaCellAddress("Sheet1", 1, 1);
            var right = new FormulaCellAddress("sheet1", 1, 1);

            Assert.Equal(left, right);
            Assert.True(left == right);
        }

        [Fact]
        public void FormulaCellAddress_HashSet_Ignores_Sheet_Case()
        {
            var addresses = new HashSet<FormulaCellAddress>
            {
                new FormulaCellAddress("Sheet1", 1, 1),
                new FormulaCellAddress("sheet1", 1, 1)
            };

            Assert.Single(addresses);
        }

        [Fact]
        public void FormulaRangeAddress_Contains_Ignores_Sheet_Case()
        {
            var range = new FormulaRangeAddress(
                new FormulaCellAddress("Sheet1", 1, 1),
                new FormulaCellAddress("Sheet1", 2, 2));

            Assert.True(range.Contains(new FormulaCellAddress("sheet1", 2, 1)));
        }
    }
}
