// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridNumericColumnDefinition : DataGridBoundColumnDefinition
    {
        private string _formatString;
        private NumberFormatInfo _numberFormat;
        private decimal? _minimum;
        private decimal? _maximum;
        private decimal? _increment;
        private bool? _showButtonSpinner;
        private Location? _buttonSpinnerLocation;
        private bool? _allowSpin;
        private bool? _clipValueToMinMax;
        private string _watermark;
        private HorizontalAlignment? _horizontalContentAlignment;
        private VerticalAlignment? _verticalContentAlignment;

        public string FormatString
        {
            get => _formatString;
            set => SetProperty(ref _formatString, value);
        }

        public NumberFormatInfo NumberFormat
        {
            get => _numberFormat;
            set => SetProperty(ref _numberFormat, value);
        }

        public decimal? Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        public decimal? Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        public decimal? Increment
        {
            get => _increment;
            set => SetProperty(ref _increment, value);
        }

        public bool? ShowButtonSpinner
        {
            get => _showButtonSpinner;
            set => SetProperty(ref _showButtonSpinner, value);
        }

        public Location? ButtonSpinnerLocation
        {
            get => _buttonSpinnerLocation;
            set => SetProperty(ref _buttonSpinnerLocation, value);
        }

        public bool? AllowSpin
        {
            get => _allowSpin;
            set => SetProperty(ref _allowSpin, value);
        }

        public bool? ClipValueToMinMax
        {
            get => _clipValueToMinMax;
            set => SetProperty(ref _clipValueToMinMax, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }

        public HorizontalAlignment? HorizontalContentAlignment
        {
            get => _horizontalContentAlignment;
            set => SetProperty(ref _horizontalContentAlignment, value);
        }

        public VerticalAlignment? VerticalContentAlignment
        {
            get => _verticalContentAlignment;
            set => SetProperty(ref _verticalContentAlignment, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridNumericColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridNumericColumn numericColumn)
            {
                numericColumn.FormatString = FormatString;
                numericColumn.NumberFormat = NumberFormat;
                numericColumn.Watermark = Watermark;

                if (Minimum.HasValue)
                {
                    numericColumn.Minimum = Minimum.Value;
                }

                if (Maximum.HasValue)
                {
                    numericColumn.Maximum = Maximum.Value;
                }

                if (Increment.HasValue)
                {
                    numericColumn.Increment = Increment.Value;
                }

                if (ShowButtonSpinner.HasValue)
                {
                    numericColumn.ShowButtonSpinner = ShowButtonSpinner.Value;
                }

                if (ButtonSpinnerLocation.HasValue)
                {
                    numericColumn.ButtonSpinnerLocation = ButtonSpinnerLocation.Value;
                }

                if (AllowSpin.HasValue)
                {
                    numericColumn.AllowSpin = AllowSpin.Value;
                }

                if (ClipValueToMinMax.HasValue)
                {
                    numericColumn.ClipValueToMinMax = ClipValueToMinMax.Value;
                }

                if (HorizontalContentAlignment.HasValue)
                {
                    numericColumn.HorizontalContentAlignment = HorizontalContentAlignment.Value;
                }

                if (VerticalContentAlignment.HasValue)
                {
                    numericColumn.VerticalContentAlignment = VerticalContentAlignment.Value;
                }
            }
        }
    }
}
