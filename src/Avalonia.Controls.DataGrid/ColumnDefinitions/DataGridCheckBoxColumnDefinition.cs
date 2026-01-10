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
    sealed class DataGridCheckBoxColumnDefinition : DataGridBoundColumnDefinition
    {
        private bool? _isThreeState;

        public bool? IsThreeState
        {
            get => _isThreeState;
            set => SetProperty(ref _isThreeState, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridCheckBoxColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridCheckBoxColumn checkBoxColumn && IsThreeState.HasValue)
            {
                checkBoxColumn.IsThreeState = IsThreeState.Value;
            }
        }
    }
}
