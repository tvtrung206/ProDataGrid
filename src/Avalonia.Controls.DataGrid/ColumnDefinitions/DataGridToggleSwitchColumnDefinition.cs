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
    sealed class DataGridToggleSwitchColumnDefinition : DataGridBoundColumnDefinition
    {
        private object _onContent;
        private object _offContent;
        private string _onContentTemplateKey;
        private string _offContentTemplateKey;
        private bool? _isThreeState;

        public object OnContent
        {
            get => _onContent;
            set => SetProperty(ref _onContent, value);
        }

        public object OffContent
        {
            get => _offContent;
            set => SetProperty(ref _offContent, value);
        }

        public string OnContentTemplateKey
        {
            get => _onContentTemplateKey;
            set => SetProperty(ref _onContentTemplateKey, value);
        }

        public string OffContentTemplateKey
        {
            get => _offContentTemplateKey;
            set => SetProperty(ref _offContentTemplateKey, value);
        }

        public bool? IsThreeState
        {
            get => _isThreeState;
            set => SetProperty(ref _isThreeState, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridToggleSwitchColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridToggleSwitchColumn toggleColumn)
            {
                toggleColumn.OnContent = OnContent;
                toggleColumn.OffContent = OffContent;
                toggleColumn.OnContentTemplate = OnContentTemplateKey != null
                    ? context?.ResolveResource<IDataTemplate>(OnContentTemplateKey)
                    : null;
                toggleColumn.OffContentTemplate = OffContentTemplateKey != null
                    ? context?.ResolveResource<IDataTemplate>(OffContentTemplateKey)
                    : null;

                if (IsThreeState.HasValue)
                {
                    toggleColumn.IsThreeState = IsThreeState.Value;
                }
            }
        }
    }
}
