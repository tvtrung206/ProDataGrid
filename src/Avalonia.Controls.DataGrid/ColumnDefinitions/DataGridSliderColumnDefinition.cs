// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using Avalonia.Controls.Primitives;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridSliderColumnDefinition : DataGridBoundColumnDefinition
    {
        private double? _minimum;
        private double? _maximum;
        private double? _smallChange;
        private double? _largeChange;
        private double? _tickFrequency;
        private bool? _isSnapToTickEnabled;
        private TickPlacement? _tickPlacement;
        private bool? _showValueText;
        private string _valueTextFormat;

        public double? Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        public double? Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        public double? SmallChange
        {
            get => _smallChange;
            set => SetProperty(ref _smallChange, value);
        }

        public double? LargeChange
        {
            get => _largeChange;
            set => SetProperty(ref _largeChange, value);
        }

        public double? TickFrequency
        {
            get => _tickFrequency;
            set => SetProperty(ref _tickFrequency, value);
        }

        public bool? IsSnapToTickEnabled
        {
            get => _isSnapToTickEnabled;
            set => SetProperty(ref _isSnapToTickEnabled, value);
        }

        public TickPlacement? TickPlacement
        {
            get => _tickPlacement;
            set => SetProperty(ref _tickPlacement, value);
        }

        public bool? ShowValueText
        {
            get => _showValueText;
            set => SetProperty(ref _showValueText, value);
        }

        public string ValueTextFormat
        {
            get => _valueTextFormat;
            set => SetProperty(ref _valueTextFormat, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridSliderColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridSliderColumn sliderColumn)
            {
                sliderColumn.ValueTextFormat = ValueTextFormat;

                if (Minimum.HasValue)
                {
                    sliderColumn.Minimum = Minimum.Value;
                }

                if (Maximum.HasValue)
                {
                    sliderColumn.Maximum = Maximum.Value;
                }

                if (SmallChange.HasValue)
                {
                    sliderColumn.SmallChange = SmallChange.Value;
                }

                if (LargeChange.HasValue)
                {
                    sliderColumn.LargeChange = LargeChange.Value;
                }

                if (TickFrequency.HasValue)
                {
                    sliderColumn.TickFrequency = TickFrequency.Value;
                }

                if (IsSnapToTickEnabled.HasValue)
                {
                    sliderColumn.IsSnapToTickEnabled = IsSnapToTickEnabled.Value;
                }

                if (TickPlacement.HasValue)
                {
                    sliderColumn.TickPlacement = TickPlacement.Value;
                }

                if (ShowValueText.HasValue)
                {
                    sliderColumn.ShowValueText = ShowValueText.Value;
                }
            }
        }
    }
}
