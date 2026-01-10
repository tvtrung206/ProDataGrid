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
    sealed class DataGridToggleButtonColumnDefinition : DataGridBoundColumnDefinition
    {
        private object _content;
        private object _checkedContent;
        private object _uncheckedContent;
        private bool? _isThreeState;
        private ClickMode? _clickMode;

        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public object CheckedContent
        {
            get => _checkedContent;
            set => SetProperty(ref _checkedContent, value);
        }

        public object UncheckedContent
        {
            get => _uncheckedContent;
            set => SetProperty(ref _uncheckedContent, value);
        }

        public bool? IsThreeState
        {
            get => _isThreeState;
            set => SetProperty(ref _isThreeState, value);
        }

        public ClickMode? ClickMode
        {
            get => _clickMode;
            set => SetProperty(ref _clickMode, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridToggleButtonColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridToggleButtonColumn toggleColumn)
            {
                toggleColumn.Content = Content;
                toggleColumn.CheckedContent = CheckedContent;
                toggleColumn.UncheckedContent = UncheckedContent;

                if (IsThreeState.HasValue)
                {
                    toggleColumn.IsThreeState = IsThreeState.Value;
                }

                if (ClickMode.HasValue)
                {
                    toggleColumn.ClickMode = ClickMode.Value;
                }
            }
        }
    }
}
