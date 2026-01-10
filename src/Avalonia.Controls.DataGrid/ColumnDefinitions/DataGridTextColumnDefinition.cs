// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using Avalonia.Media;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridTextColumnDefinition : DataGridBoundColumnDefinition
    {
        private FontFamily _fontFamily;
        private double? _fontSize;
        private FontStyle? _fontStyle;
        private FontWeight? _fontWeight;
        private FontStretch? _fontStretch;
        private IBrush _foreground;
        private string _watermark;

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double? FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public FontStyle? FontStyle
        {
            get => _fontStyle;
            set => SetProperty(ref _fontStyle, value);
        }

        public FontWeight? FontWeight
        {
            get => _fontWeight;
            set => SetProperty(ref _fontWeight, value);
        }

        public FontStretch? FontStretch
        {
            get => _fontStretch;
            set => SetProperty(ref _fontStretch, value);
        }

        public IBrush Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridTextColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridTextColumn textColumn)
            {
                if (FontFamily != null)
                {
                    textColumn.FontFamily = FontFamily;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.FontFamilyProperty);
                }

                if (Foreground != null)
                {
                    textColumn.Foreground = Foreground;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.ForegroundProperty);
                }

                if (Watermark != null)
                {
                    textColumn.Watermark = Watermark;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.WatermarkProperty);
                }

                if (FontSize.HasValue)
                {
                    textColumn.FontSize = FontSize.Value;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.FontSizeProperty);
                }

                if (FontStyle.HasValue)
                {
                    textColumn.FontStyle = FontStyle.Value;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.FontStyleProperty);
                }

                if (FontWeight.HasValue)
                {
                    textColumn.FontWeight = FontWeight.Value;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.FontWeightProperty);
                }

                if (FontStretch.HasValue)
                {
                    textColumn.FontStretch = FontStretch.Value;
                }
                else
                {
                    textColumn.ClearValue(DataGridTextColumn.FontStretchProperty);
                }
            }
        }
    }
}
