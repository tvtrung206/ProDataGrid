using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Utils
{
    public class DataGridErrorTests
    {
        public static IEnumerable<object[]> DataGridErrorCases()
        {
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.CannotChangeItemsWhenLoadingRows()),
                typeof(InvalidOperationException),
                "Items cannot be added, removed or reset while rows are loading or unloading.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.CannotChangeColumnCollectionWhileAdjustingDisplayIndexes()),
                typeof(InvalidOperationException),
                "Column collection cannot be changed while adjusting display indexes.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ColumnCannotBeCollapsed()),
                typeof(InvalidOperationException),
                "Column cannot be collapsed.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ColumnCannotBeReassignedToDifferentDataGrid()),
                typeof(InvalidOperationException),
                "Column already belongs to a DataGrid instance and cannot be reassigned.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ColumnNotInThisDataGrid()),
                typeof(ArgumentException),
                "Provided column does not belong to this DataGrid.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ItemIsNotContainedInTheItemsSource("item")),
                typeof(ArgumentException),
                "The item is not contained in the ItemsSource.",
                "item"
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.NoCurrentRow()),
                typeof(InvalidOperationException),
                "There is no current row.  Operation cannot be completed.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.NoOwningGrid(typeof(DataGrid))),
                typeof(InvalidOperationException),
                "There is no instance of DataGrid assigned to this Avalonia.Controls.DataGrid.  Operation cannot be completed.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.UnderlyingPropertyIsReadOnly("Width")),
                typeof(InvalidOperationException),
                "Width cannot be set because the underlying property is read only.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueCannotBeSetToInfinity("width")),
                typeof(ArgumentException),
                "width cannot be set to infinity.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueCannotBeSetToNAN("height")),
                typeof(ArgumentException),
                "height cannot be set to double.NAN.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueCannotBeSetToNull("value", "Value")),
                typeof(ArgumentNullException),
                "Value cannot be set to a null value.",
                "value"
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueIsNotAnInstanceOf("value", typeof(DataGrid))),
                typeof(ArgumentException),
                null!,
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueIsNotAnInstanceOfEitherOr("value", typeof(DataGrid), typeof(string))),
                typeof(ArgumentException),
                null!,
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("height", "Height", 1)),
                typeof(ArgumentOutOfRangeException),
                "Height must be greater than or equal to 1.",
                "height"
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueMustBeLessThanOrEqualTo("height", "Height", 10)),
                typeof(ArgumentOutOfRangeException),
                "Height must be less than or equal to 10.",
                "height"
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGrid.ValueMustBeLessThan("height", "Height", 10)),
                typeof(ArgumentOutOfRangeException),
                "Height must be less than 10.",
                "height"
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridColumnHeader.ContentDoesNotSupportUIElements()),
                typeof(NotSupportedException),
                "Content does not support Controls; use ContentTemplate instead.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridLength.InvalidUnitType("length")),
                typeof(ArgumentException),
                "length is not a valid DataGridLengthUnitType.",
                "length"
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridLengthConverter.CannotConvertFrom("string")),
                typeof(NotSupportedException),
                "DataGridLengthConverter cannot convert from string.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridLengthConverter.CannotConvertTo("string")),
                typeof(NotSupportedException),
                "Cannot convert from DataGridLength to string.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridLengthConverter.InvalidDataGridLength("value")),
                typeof(NotSupportedException),
                "Invalid DataGridLength.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridRow.InvalidRowIndexCannotCompleteOperation()),
                typeof(InvalidOperationException),
                "Invalid row index. Operation cannot be completed.",
                null!
            };
            yield return new object[]
            {
                (Func<Exception>)(() => DataGridError.DataGridSelectedItemsCollection.CannotChangeSelectedItemsCollectionInSingleMode()),
                typeof(InvalidOperationException),
                "Can only change SelectedItems collection in Extended selection mode.  Use SelectedItem property in Single selection mode.",
                null!
            };
        }

        [Theory]
        [MemberData(nameof(DataGridErrorCases))]
        public void DataGridError_Returns_Expected_Exception(
            Func<Exception> factory,
            Type expectedType,
            string? expectedMessage,
            string? expectedParamName)
        {
            var exception = factory();

            Assert.IsType(expectedType, exception);

            if (!string.IsNullOrEmpty(expectedMessage))
            {
                Assert.Contains(expectedMessage, exception.Message);
            }

            if (expectedParamName != null)
            {
                var argumentException = Assert.IsAssignableFrom<ArgumentException>(exception);
                Assert.Equal(expectedParamName, argumentException.ParamName);
            }
        }

        [Theory]
        [InlineData(true, true, "Height must be greater than or equal to 1 and less than or equal to 10.")]
        [InlineData(true, false, "Height must be greater than or equal to 1 and less than 10.")]
        [InlineData(false, true, "Height must be greater than 1 and less than or equal to 10.")]
        [InlineData(false, false, "Height must be greater than 1 and less than 10.")]
        public void ValueMustBeBetween_Formats_Message(
            bool lowInclusive,
            bool highInclusive,
            string expectedMessage)
        {
            var exception = DataGridError.DataGrid.ValueMustBeBetween(
                "height",
                "Height",
                1,
                lowInclusive,
                10,
                highInclusive);

            var argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);

            Assert.Equal("height", argumentException.ParamName);
            Assert.Contains(expectedMessage, argumentException.Message);
        }

        [Fact]
        public void MissingTemplateForType_Uses_TypeName_Field()
        {
            var exception = DataGridError.DataGridTemplateColumn.MissingTemplateForType(typeof(DataGrid));

            Assert.IsType<TypeInitializationException>(exception);
            Assert.Equal("Missing template.  Cannot initialize Avalonia.Controls.DataGrid.", exception.TypeName);
        }
    }
}
