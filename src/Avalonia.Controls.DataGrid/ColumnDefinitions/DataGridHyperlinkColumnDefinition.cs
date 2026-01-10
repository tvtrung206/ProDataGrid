// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridHyperlinkColumnDefinition : DataGridBoundColumnDefinition
    {
        private string _targetName;
        private string _watermark;
        private DataGridBindingDefinition _contentBinding;

        public string TargetName
        {
            get => _targetName;
            set => SetProperty(ref _targetName, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }

        public DataGridBindingDefinition ContentBinding
        {
            get => _contentBinding;
            set => SetProperty(ref _contentBinding, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridHyperlinkColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridHyperlinkColumn hyperlinkColumn)
            {
                hyperlinkColumn.TargetName = TargetName;
                hyperlinkColumn.Watermark = Watermark;
                hyperlinkColumn.ContentBinding = ContentBinding?.CreateBinding();
            }
        }
    }
}
