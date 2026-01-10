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
    sealed class DataGridImageColumnDefinition : DataGridBoundColumnDefinition
    {
        private double? _imageWidth;
        private double? _imageHeight;
        private Stretch? _stretch;
        private StretchDirection? _stretchDirection;
        private bool? _allowEditing;
        private string _watermark;

        public double? ImageWidth
        {
            get => _imageWidth;
            set => SetProperty(ref _imageWidth, value);
        }

        public double? ImageHeight
        {
            get => _imageHeight;
            set => SetProperty(ref _imageHeight, value);
        }

        public Stretch? Stretch
        {
            get => _stretch;
            set => SetProperty(ref _stretch, value);
        }

        public StretchDirection? StretchDirection
        {
            get => _stretchDirection;
            set => SetProperty(ref _stretchDirection, value);
        }

        public bool? AllowEditing
        {
            get => _allowEditing;
            set => SetProperty(ref _allowEditing, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridImageColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridImageColumn imageColumn)
            {
                imageColumn.Watermark = Watermark;

                if (ImageWidth.HasValue)
                {
                    imageColumn.ImageWidth = ImageWidth.Value;
                }

                if (ImageHeight.HasValue)
                {
                    imageColumn.ImageHeight = ImageHeight.Value;
                }

                if (Stretch.HasValue)
                {
                    imageColumn.Stretch = Stretch.Value;
                }

                if (StretchDirection.HasValue)
                {
                    imageColumn.StretchDirection = StretchDirection.Value;
                }

                if (AllowEditing.HasValue)
                {
                    imageColumn.AllowEditing = AllowEditing.Value;
                }
            }
        }
    }
}
