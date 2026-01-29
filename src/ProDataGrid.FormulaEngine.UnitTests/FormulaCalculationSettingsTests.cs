// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Globalization;
using Xunit;

namespace ProDataGrid.FormulaEngine.Tests
{
    public sealed class FormulaCalculationSettingsTests
    {
        [Fact]
        public void CreateParseOptions_Uses_Culture_Separators()
        {
            var settings = new FormulaCalculationSettings
            {
                Culture = new CultureInfo("de-DE")
            };

            var options = settings.CreateParseOptions();

            Assert.Equal(';', options.ArgumentSeparator);
            Assert.Equal(',', options.DecimalSeparator);
        }
    }
}
