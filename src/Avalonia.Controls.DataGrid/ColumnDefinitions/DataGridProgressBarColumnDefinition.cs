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
    sealed class DataGridProgressBarColumnDefinition : DataGridBoundColumnDefinition
    {
        private double? _minimum;
        private double? _maximum;
        private bool? _showProgressText;
        private string _progressTextFormat;
        private IBrush _foreground;
        private IBrush _background;
        private double? _height;

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

        public bool? ShowProgressText
        {
            get => _showProgressText;
            set => SetProperty(ref _showProgressText, value);
        }

        public string ProgressTextFormat
        {
            get => _progressTextFormat;
            set => SetProperty(ref _progressTextFormat, value);
        }

        public IBrush Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        public IBrush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        public double? Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridProgressBarColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridProgressBarColumn progressColumn)
            {
                progressColumn.ProgressTextFormat = ProgressTextFormat;
                progressColumn.Foreground = Foreground;
                progressColumn.Background = Background;

                if (Minimum.HasValue)
                {
                    progressColumn.Minimum = Minimum.Value;
                }

                if (Maximum.HasValue)
                {
                    progressColumn.Maximum = Maximum.Value;
                }

                if (ShowProgressText.HasValue)
                {
                    progressColumn.ShowProgressText = ShowProgressText.Value;
                }

                if (Height.HasValue)
                {
                    progressColumn.Height = Height.Value;
                }
            }
        }
    }
}
