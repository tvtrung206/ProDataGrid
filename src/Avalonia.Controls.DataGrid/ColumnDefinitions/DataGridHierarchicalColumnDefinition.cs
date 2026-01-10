// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using Avalonia.Controls.Templates;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridHierarchicalColumnDefinition : DataGridBoundColumnDefinition
    {
        private double? _indent;
        private string _cellTemplateKey;

        public double? Indent
        {
            get => _indent;
            set => SetProperty(ref _indent, value);
        }

        public string CellTemplateKey
        {
            get => _cellTemplateKey;
            set => SetProperty(ref _cellTemplateKey, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridHierarchicalColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridHierarchicalColumn hierarchicalColumn)
            {
                hierarchicalColumn.CellTemplate = CellTemplateKey != null
                    ? context?.ResolveResource<IDataTemplate>(CellTemplateKey)
                    : null;

                if (Indent.HasValue)
                {
                    hierarchicalColumn.Indent = Indent.Value;
                }
            }
        }
    }
}
