// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Threading;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    partial class DataGrid
    {
        private void SetColumnDefinitionsSource(IList<DataGridColumnDefinition> value)
        {
            if (_areHandlersSuspended)
            {
                return;
            }

            if (ReferenceEquals(value, _columnDefinitionsSource))
            {
                return;
            }

            SetAndRaise(ColumnDefinitionsSourceProperty, ref _columnDefinitionsSource, value);
        }

        private void OnColumnDefinitionsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (_areHandlersSuspended)
            {
                return;
            }

            var oldValue = (IList<DataGridColumnDefinition>)e.OldValue;
            var newValue = (IList<DataGridColumnDefinition>)e.NewValue;

            if (ReferenceEquals(oldValue, newValue))
            {
                return;
            }

            DetachColumnDefinitions(oldValue);

            if (newValue == null)
            {
                _pendingColumnDefinitionsApply = false;
                ClearColumnsForBinding();
                return;
            }

            try
            {
                if (_boundColumns != null)
                {
                    throw new InvalidOperationException("Cannot set ColumnDefinitionsSource when Columns are bound. Clear Columns before binding definitions.");
                }

                if (HasInlineColumnsDefinedExcludingDefinitions())
                {
                    throw new InvalidOperationException("Cannot bind ColumnDefinitionsSource when inline columns are already defined. Clear existing columns before binding.");
                }

                AttachColumnDefinitions(newValue);

                if (_autoGeneratingColumnOperationCount > 0)
                {
                    _pendingColumnDefinitionsApply = true;
                    return;
                }

                ApplyColumnDefinitionsSnapshot();
            }
            catch
            {
                _columnDefinitionsSource = oldValue;
                AttachColumnDefinitions(oldValue);
                throw;
            }
        }

        private void AttachColumnDefinitions(IList<DataGridColumnDefinition> definitions)
        {
            _columnDefinitionsNotifications = definitions as INotifyCollectionChanged;
            if (_columnDefinitionsNotifications != null)
            {
                _columnDefinitionsNotifications.CollectionChanged += ColumnDefinitions_CollectionChanged;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ColumnDefinitionsSource does not implement INotifyCollectionChanged; applying snapshot without live updates.");
            }

            _columnDefinitionsThreadId = Environment.CurrentManagedThreadId;
        }

        private void DetachColumnDefinitions(IList<DataGridColumnDefinition> definitions)
        {
            if (_columnDefinitionsNotifications != null)
            {
                _columnDefinitionsNotifications.CollectionChanged -= ColumnDefinitions_CollectionChanged;
            }

            RemoveDefinitionColumns();

            foreach (var definition in _columnDefinitionMap.Keys.ToList())
            {
                UnsubscribeDefinition(definition);
            }

            foreach (var column in _columnDefinitionMap.Values)
            {
                DataGridColumnMetadata.ClearDefinition(column);
            }

            _columnDefinitionsNotifications = null;
            _columnDefinitionsThreadId = null;
            _columnDefinitionMap.Clear();
            _definitionColumns.Clear();
        }

        private void ColumnDefinitions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                throw new InvalidOperationException("ColumnDefinitionsSource changes must occur on the UI thread.");
            }

            if (_columnDefinitionsThreadId.HasValue && _columnDefinitionsThreadId.Value != Environment.CurrentManagedThreadId)
            {
                throw new InvalidOperationException("ColumnDefinitionsSource changes must occur on the same thread that created the binding.");
            }

            if (_syncingColumnDefinitions)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset && ColumnsSourceResetBehavior == ColumnsSourceResetBehavior.Ignore)
            {
                return;
            }

            _syncingColumnDefinitions = true;
            try
            {
                ApplyColumnDefinitionsSnapshot();
            }
            finally
            {
                _syncingColumnDefinitions = false;
            }
        }

        private void ColumnDefinition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_syncingColumnDefinitions)
            {
                return;
            }

            if (sender is DataGridColumnDefinition definition && _columnDefinitionMap.TryGetValue(definition, out var column))
            {
                definition.ApplyToColumn(column, new DataGridColumnDefinitionContext(this));
            }
        }

        private void SubscribeDefinition(DataGridColumnDefinition definition)
        {
            if (definition is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += ColumnDefinition_PropertyChanged;
            }
        }

        private void UnsubscribeDefinition(DataGridColumnDefinition definition)
        {
            if (definition is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged -= ColumnDefinition_PropertyChanged;
            }
        }

        private void ApplyColumnDefinitionsSnapshot()
        {
            if (_columnDefinitionsSource == null)
            {
                return;
            }

            List<DataGridColumnDefinition> snapshot;
            try
            {
                snapshot = _columnDefinitionsSource.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to enumerate ColumnDefinitionsSource: {ex}");
                throw new InvalidOperationException("Failed to enumerate ColumnDefinitionsSource.", ex);
            }

            _areHandlersSuspended = true;
            _syncingColumnDefinitions = true;
            try
            {
                var context = new DataGridColumnDefinitionContext(this);
                var autoColumns = ColumnsInternal.Where(c => c.IsAutoGenerated).ToList();
                var newColumns = new List<DataGridColumn>(snapshot.Count);
                var newMap = new Dictionary<DataGridColumnDefinition, DataGridColumn>();
                var newColumnSet = new HashSet<DataGridColumn>();

                foreach (var definition in snapshot)
                {
                    if (definition == null)
                    {
                        throw new ArgumentNullException(nameof(definition));
                    }

                    if (!_columnDefinitionMap.TryGetValue(definition, out var column))
                    {
                        column = definition.CreateColumn(context);
                        SubscribeDefinition(definition);
                    }
                    else
                    {
                        definition.ApplyToColumn(column, context);
                    }

                    DataGridColumnMetadata.SetDefinition(column, definition);

                    newColumns.Add(column);
                    newMap[definition] = column;
                    newColumnSet.Add(column);
                }

                foreach (var pair in _columnDefinitionMap)
                {
                    if (!newMap.ContainsKey(pair.Key))
                    {
                        UnsubscribeDefinition(pair.Key);
                        DataGridColumnMetadata.ClearDefinition(pair.Value);
                    }
                }

                _columnDefinitionMap.Clear();
                foreach (var pair in newMap)
                {
                    _columnDefinitionMap.Add(pair.Key, pair.Value);
                }

                _definitionColumns.Clear();
                foreach (var column in newColumnSet)
                {
                    _definitionColumns.Add(column);
                }

                ClearColumnsForBinding();

                if (AutoGeneratedColumnsPlacement == AutoGeneratedColumnsPlacement.BeforeSource)
                {
                    foreach (var auto in autoColumns)
                    {
                        ColumnsInternal.Add(auto);
                    }
                }

                foreach (var column in newColumns)
                {
                    ValidateBoundColumn(column);
                    ColumnsInternal.Add(column);
                }

                if (AutoGeneratedColumnsPlacement == AutoGeneratedColumnsPlacement.AfterSource)
                {
                    foreach (var auto in autoColumns)
                    {
                        ColumnsInternal.Add(auto);
                    }
                }

                ApplyDefinitionDisplayIndexes(autoColumns, newColumns);
            }
            finally
            {
                _syncingColumnDefinitions = false;
                _areHandlersSuspended = false;
            }
        }

        private void ApplyDefinitionDisplayIndexes(IReadOnlyList<DataGridColumn> autoColumns, IReadOnlyList<DataGridColumn> definitionColumns)
        {
            _areHandlersSuspended = true;
            try
            {
                var displayIndex = 0;

                if (AutoGeneratedColumnsPlacement == AutoGeneratedColumnsPlacement.BeforeSource)
                {
                    foreach (var auto in autoColumns)
                    {
                        auto.DisplayIndex = displayIndex++;
                    }
                }

                foreach (var column in definitionColumns)
                {
                    if (column != null && ColumnsInternal.Contains(column))
                    {
                        column.DisplayIndex = displayIndex++;
                    }
                }

                if (AutoGeneratedColumnsPlacement == AutoGeneratedColumnsPlacement.AfterSource)
                {
                    foreach (var auto in autoColumns)
                    {
                        auto.DisplayIndex = displayIndex++;
                    }
                }
            }
            finally
            {
                _areHandlersSuspended = false;
            }
        }

        private void ApplyPendingColumnDefinitions()
        {
            if (!_pendingColumnDefinitionsApply)
            {
                return;
            }

            _pendingColumnDefinitionsApply = false;
            ApplyColumnDefinitionsSnapshot();
        }

        private bool HasInlineColumnsDefinedExcludingDefinitions()
        {
            return ColumnsInternal.Any(c =>
                !c.IsAutoGenerated &&
                c is not DataGridFillerColumn &&
                !_definitionColumns.Contains(c) &&
                (_boundColumns == null || !_boundColumns.Contains(c)));
        }

        private bool HasColumnsSource()
        {
            return _boundColumns != null || _columnDefinitionsSource != null;
        }

        private void RemoveDefinitionColumns()
        {
            if (_definitionColumns.Count == 0)
            {
                return;
            }

            _syncingInternalColumns = true;
            _areHandlersSuspended = true;
            try
            {
                foreach (var column in _definitionColumns)
                {
                    if (column != null && ColumnsInternal.Contains(column))
                    {
                        ColumnsInternal.Remove(column);
                    }
                }
            }
            finally
            {
                _areHandlersSuspended = false;
                _syncingInternalColumns = false;
            }
        }
    }
}
