// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridAutoCompleteColumnDefinition : DataGridBoundColumnDefinition
    {
        private IEnumerable _itemsSource;
        private string _itemTemplateKey;
        private AutoCompleteFilterMode? _filterMode;
        private int? _minimumPrefixLength;
        private TimeSpan? _minimumPopulateDelay;
        private double? _maxDropDownHeight;
        private bool? _isTextCompletionEnabled;
        private string _watermark;

        public IEnumerable ItemsSource
        {
            get => _itemsSource;
            set => SetProperty(ref _itemsSource, value);
        }

        public string ItemTemplateKey
        {
            get => _itemTemplateKey;
            set => SetProperty(ref _itemTemplateKey, value);
        }

        public AutoCompleteFilterMode? FilterMode
        {
            get => _filterMode;
            set => SetProperty(ref _filterMode, value);
        }

        public int? MinimumPrefixLength
        {
            get => _minimumPrefixLength;
            set => SetProperty(ref _minimumPrefixLength, value);
        }

        public TimeSpan? MinimumPopulateDelay
        {
            get => _minimumPopulateDelay;
            set => SetProperty(ref _minimumPopulateDelay, value);
        }

        public double? MaxDropDownHeight
        {
            get => _maxDropDownHeight;
            set => SetProperty(ref _maxDropDownHeight, value);
        }

        public bool? IsTextCompletionEnabled
        {
            get => _isTextCompletionEnabled;
            set => SetProperty(ref _isTextCompletionEnabled, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridAutoCompleteColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridAutoCompleteColumn autoColumn)
            {
                autoColumn.ItemsSource = ItemsSource;
                autoColumn.ItemTemplate = ItemTemplateKey != null
                    ? context?.ResolveResource<IDataTemplate>(ItemTemplateKey)
                    : null;
                autoColumn.Watermark = Watermark;

                if (FilterMode.HasValue)
                {
                    autoColumn.FilterMode = FilterMode.Value;
                }

                if (MinimumPrefixLength.HasValue)
                {
                    autoColumn.MinimumPrefixLength = MinimumPrefixLength.Value;
                }

                if (MinimumPopulateDelay.HasValue)
                {
                    autoColumn.MinimumPopulateDelay = MinimumPopulateDelay.Value;
                }

                if (MaxDropDownHeight.HasValue)
                {
                    autoColumn.MaxDropDownHeight = MaxDropDownHeight.Value;
                }

                if (IsTextCompletionEnabled.HasValue)
                {
                    autoColumn.IsTextCompletionEnabled = IsTextCompletionEnabled.Value;
                }
            }
        }
    }
}
