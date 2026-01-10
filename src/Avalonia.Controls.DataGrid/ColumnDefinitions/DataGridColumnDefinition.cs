// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Styling;

namespace Avalonia.Controls
{
    #if !DATAGRID_INTERNAL
    public
    #else
    internal
    #endif
    sealed class DataGridColumnDefinitionContext
    {
        public DataGridColumnDefinitionContext(DataGrid grid)
        {
            Grid = grid;
        }

        public DataGrid Grid { get; }

        public T ResolveResource<T>(string key) where T : class
        {
            if (Grid == null || string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (Grid.TryFindResource(key, out var resource) && resource is T typed)
            {
                return typed;
            }

            if (Application.Current != null && Application.Current.TryFindResource(key, out resource) && resource is T appTyped)
            {
                return appTyped;
            }

            return null;
        }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    abstract class DataGridColumnDefinition : INotifyPropertyChanged
    {
        private object _header;
        private string _headerTemplateKey;
        private string _headerThemeKey;
        private string _cellThemeKey;
        private string _filterThemeKey;
        private IList<string> _cellStyleClasses;
        private IList<string> _headerStyleClasses;
        private DataGridBindingDefinition _cellBackgroundBinding;
        private DataGridBindingDefinition _cellForegroundBinding;
        private bool? _canUserSort;
        private bool? _canUserResize;
        private bool? _canUserReorder;
        private bool? _isReadOnly;
        private bool? _isVisible;
        private bool? _showFilterButton;
        private int? _displayIndex;
        private DataGridLength? _width;
        private double? _minWidth;
        private double? _maxWidth;
        private ListSortDirection? _sortDirection;
        private string _sortMemberPath;
        private object _tag;
        private System.Collections.IComparer _customSortComparer;
        private IDataGridColumnValueAccessor _valueAccessor;
        private Type _valueType;

        public event PropertyChangedEventHandler PropertyChanged;

        public object Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public string HeaderTemplateKey
        {
            get => _headerTemplateKey;
            set => SetProperty(ref _headerTemplateKey, value);
        }

        public string HeaderThemeKey
        {
            get => _headerThemeKey;
            set => SetProperty(ref _headerThemeKey, value);
        }

        public string CellThemeKey
        {
            get => _cellThemeKey;
            set => SetProperty(ref _cellThemeKey, value);
        }

        public string FilterThemeKey
        {
            get => _filterThemeKey;
            set => SetProperty(ref _filterThemeKey, value);
        }

        public IList<string> CellStyleClasses
        {
            get => _cellStyleClasses;
            set => SetProperty(ref _cellStyleClasses, value);
        }

        public IList<string> HeaderStyleClasses
        {
            get => _headerStyleClasses;
            set => SetProperty(ref _headerStyleClasses, value);
        }

        public DataGridBindingDefinition CellBackgroundBinding
        {
            get => _cellBackgroundBinding;
            set => SetProperty(ref _cellBackgroundBinding, value);
        }

        public DataGridBindingDefinition CellForegroundBinding
        {
            get => _cellForegroundBinding;
            set => SetProperty(ref _cellForegroundBinding, value);
        }

        public bool? CanUserSort
        {
            get => _canUserSort;
            set => SetProperty(ref _canUserSort, value);
        }

        public bool? CanUserResize
        {
            get => _canUserResize;
            set => SetProperty(ref _canUserResize, value);
        }

        public bool? CanUserReorder
        {
            get => _canUserReorder;
            set => SetProperty(ref _canUserReorder, value);
        }

        public bool? IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }

        public bool? IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool? ShowFilterButton
        {
            get => _showFilterButton;
            set => SetProperty(ref _showFilterButton, value);
        }

        public int? DisplayIndex
        {
            get => _displayIndex;
            set => SetProperty(ref _displayIndex, value);
        }

        public DataGridLength? Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double? MinWidth
        {
            get => _minWidth;
            set => SetProperty(ref _minWidth, value);
        }

        public double? MaxWidth
        {
            get => _maxWidth;
            set => SetProperty(ref _maxWidth, value);
        }

        public ListSortDirection? SortDirection
        {
            get => _sortDirection;
            set => SetProperty(ref _sortDirection, value);
        }

        public string SortMemberPath
        {
            get => _sortMemberPath;
            set => SetProperty(ref _sortMemberPath, value);
        }

        public object Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }

        public System.Collections.IComparer CustomSortComparer
        {
            get => _customSortComparer;
            set => SetProperty(ref _customSortComparer, value);
        }

        public IDataGridColumnValueAccessor ValueAccessor
        {
            get => _valueAccessor;
            set => SetProperty(ref _valueAccessor, value);
        }

        public Type ValueType
        {
            get => _valueType;
            set => SetProperty(ref _valueType, value);
        }

        internal DataGridColumn CreateColumn(DataGridColumnDefinitionContext context)
        {
            var column = CreateColumnCore();
            ApplyToColumn(column, context);
            return column;
        }

        internal void ApplyToColumn(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            ApplyBaseProperties(column, context);
            ApplyColumnProperties(column, context);
        }

        protected abstract DataGridColumn CreateColumnCore();

        protected abstract void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context);

        protected virtual void ApplyBaseProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            column.Header = Header;
            column.SortMemberPath = SortMemberPath;
            column.Tag = Tag;
            column.CustomSortComparer = CustomSortComparer;

            if (HeaderTemplateKey != null)
            {
                column.HeaderTemplate = context?.ResolveResource<IDataTemplate>(HeaderTemplateKey);
            }
            else
            {
                column.HeaderTemplate = null;
            }

            if (HeaderThemeKey != null)
            {
                column.HeaderTheme = context?.ResolveResource<ControlTheme>(HeaderThemeKey);
            }
            else
            {
                column.HeaderTheme = null;
            }

            if (CellThemeKey != null)
            {
                column.CellTheme = context?.ResolveResource<ControlTheme>(CellThemeKey);
            }
            else
            {
                column.CellTheme = null;
            }

            if (FilterThemeKey != null)
            {
                column.FilterTheme = context?.ResolveResource<ControlTheme>(FilterThemeKey);
            }
            else
            {
                column.FilterTheme = null;
            }

            if (CellStyleClasses != null)
            {
                column.CellStyleClasses.Replace(CellStyleClasses);
            }
            else
            {
                column.CellStyleClasses.Clear();
            }

            if (HeaderStyleClasses != null)
            {
                column.HeaderStyleClasses.Replace(HeaderStyleClasses);
            }
            else
            {
                column.HeaderStyleClasses.Clear();
            }

            column.CellBackgroundBinding = CellBackgroundBinding?.CreateBinding();
            column.CellForegroundBinding = CellForegroundBinding?.CreateBinding();

            if (CanUserSort.HasValue)
            {
                column.CanUserSort = CanUserSort.Value;
            }

            if (CanUserResize.HasValue)
            {
                column.CanUserResize = CanUserResize.Value;
            }

            if (CanUserReorder.HasValue)
            {
                column.CanUserReorder = CanUserReorder.Value;
            }

            if (IsReadOnly.HasValue)
            {
                column.IsReadOnly = IsReadOnly.Value;
            }

            if (IsVisible.HasValue)
            {
                column.IsVisible = IsVisible.Value;
            }

            if (ShowFilterButton.HasValue)
            {
                column.ShowFilterButton = ShowFilterButton.Value;
            }

            if (DisplayIndex.HasValue)
            {
                column.DisplayIndex = DisplayIndex.Value;
            }

            if (Width.HasValue)
            {
                column.Width = Width.Value;
            }

            if (MinWidth.HasValue)
            {
                column.MinWidth = MinWidth.Value;
            }

            if (MaxWidth.HasValue)
            {
                column.MaxWidth = MaxWidth.Value;
            }

            if (SortDirection.HasValue)
            {
                column.SortDirection = SortDirection.Value;
            }

            if (ValueAccessor != null)
            {
                DataGridColumnMetadata.SetValueAccessor(column, ValueAccessor);
            }
            else
            {
                DataGridColumnMetadata.ClearValueAccessor(column);
            }

            if (ValueType != null)
            {
                DataGridColumnMetadata.SetValueType(column, ValueType);
            }
            else
            {
                DataGridColumnMetadata.ClearValueType(column);
            }
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
