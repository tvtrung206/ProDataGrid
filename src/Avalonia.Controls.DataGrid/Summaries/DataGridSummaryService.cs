// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Collections;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Avalonia.Controls
{
    /// <summary>
    /// Service for managing summary calculations in a DataGrid.
    /// </summary>
    internal class DataGridSummaryService : IDisposable
    {
        private readonly DataGrid _owner;
        private readonly DataGridSummaryCache _cache;
        private DispatcherTimer? _debounceTimer;
        private bool _isRecalculationPending;
        private bool _disposed;
        private int _debounceDelayMs = 100;

        /// <summary>
        /// Creates a new summary service for the specified DataGrid.
        /// </summary>
        public DataGridSummaryService(DataGrid owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _cache = new DataGridSummaryCache();
        }

        /// <summary>
        /// Event raised when summary values are recalculated.
        /// </summary>
        public event EventHandler<DataGridSummaryRecalculatedEventArgs>? SummaryRecalculated;

        /// <summary>
        /// Gets or sets the debounce delay for recalculations in milliseconds.
        /// </summary>
        public int DebounceDelayMs
        {
            get => _debounceDelayMs;
            set
            {
                if (_debounceDelayMs == value)
                {
                    return;
                }

                _debounceDelayMs = value;

                if (_debounceTimer != null)
                {
                    if (_debounceDelayMs <= 0)
                    {
                        _debounceTimer.Stop();
                    }
                    else
                    {
                        _debounceTimer.Interval = TimeSpan.FromMilliseconds(_debounceDelayMs);
                    }
                }
            }
        }

        /// <summary>
        /// Recalculates all total summaries.
        /// </summary>
        public void RecalculateTotalSummaries()
        {
            if (_disposed) return;

            _cache.InvalidateTotals();

            var items = GetAllItems();

            foreach (var column in _owner.Columns.OfType<DataGridColumn>())
            {
                RecalculateColumnSummaries(column, items, DataGridSummaryScope.Total, null);
            }

            RaiseSummaryRecalculated(DataGridSummaryScope.Total, null);
        }

        /// <summary>
        /// Recalculates summaries for a specific group.
        /// </summary>
        public void RecalculateGroupSummaries(DataGridCollectionViewGroup group)
        {
            if (_disposed || group == null) return;

            _cache.InvalidateGroup(group);

            var items = GetGroupItems(group);

            foreach (var column in _owner.Columns.OfType<DataGridColumn>())
            {
                RecalculateColumnSummaries(column, items, DataGridSummaryScope.Group, group);
            }

            RaiseSummaryRecalculated(DataGridSummaryScope.Group, group);
        }

        /// <summary>
        /// Recalculates all group summaries.
        /// </summary>
        public void RecalculateAllGroupSummaries()
        {
            if (_disposed) return;

            _cache.InvalidateGroups();

            var groups = GetAllGroups();
            foreach (var group in groups)
            {
                var items = GetGroupItems(group);

                foreach (var column in _owner.Columns.OfType<DataGridColumn>())
                {
                    RecalculateColumnSummaries(column, items, DataGridSummaryScope.Group, group);
                }
            }

            RaiseSummaryRecalculated(DataGridSummaryScope.Group, null);
        }

        /// <summary>
        /// Recalculates all summaries.
        /// </summary>
        public void RecalculateAll()
        {
            if (_disposed) return;

            RecalculateTotalSummaries();
            RecalculateAllGroupSummaries();
        }

        /// <summary>
        /// Schedules a debounced recalculation.
        /// </summary>
        public void ScheduleRecalculation()
        {
            if (_disposed) return;

            if (DebounceDelayMs <= 0)
            {
                _debounceTimer?.Stop();
                RecalculateAll();
                return;
            }

            if (_debounceTimer == null)
            {
                _debounceTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(DebounceDelayMs)
                };
                _debounceTimer.Tick += OnDebounceTimerTick;
            }

            _isRecalculationPending = true;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void OnDebounceTimerTick(object? sender, EventArgs e)
        {
            _debounceTimer?.Stop();

            if (_isRecalculationPending && !_disposed)
            {
                _isRecalculationPending = false;
                RecalculateAll();
            }
        }

        /// <summary>
        /// Gets the cached summary value for a column at total scope.
        /// </summary>
        public object? GetTotalSummaryValue(DataGridColumn column, DataGridSummaryDescription description)
        {
            if (_cache.TryGet(column, description, DataGridSummaryScope.Total, null, out var cached))
            {
                return cached;
            }

            // Calculate on demand
            var items = GetAllItems();
            var value = description.Calculate(items, column);
            _cache.Set(column, description, DataGridSummaryScope.Total, null, value);
            return value;
        }

        /// <summary>
        /// Gets the cached summary value for a column at group scope.
        /// </summary>
        public object? GetGroupSummaryValue(DataGridColumn column, DataGridSummaryDescription description, DataGridCollectionViewGroup group)
        {
            if (_cache.TryGet(column, description, DataGridSummaryScope.Group, group, out var cached))
            {
                return cached;
            }

            // Calculate on demand
            var items = GetGroupItems(group);
            var value = description.Calculate(items, column);
            _cache.Set(column, description, DataGridSummaryScope.Group, group, value);
            return value;
        }

        /// <summary>
        /// Invalidates all cached summary values.
        /// </summary>
        public void InvalidateAll()
        {
            _cache.InvalidateAll();
        }

        /// <summary>
        /// Invalidates cached values for a specific column.
        /// </summary>
        public void InvalidateColumn(DataGridColumn column)
        {
            _cache.InvalidateColumn(column);
        }

        /// <summary>
        /// Handles collection changed events for incremental updates or full recalculation.
        /// </summary>
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_disposed) return;

            // For now, just schedule a full recalculation
            // TODO: Implement incremental updates for Add/Remove operations
            ScheduleRecalculation();
        }

        /// <summary>
        /// Handles filter changes by recalculating all summaries.
        /// </summary>
        public void OnFilterChanged()
        {
            if (_disposed) return;

            InvalidateAll();
            ScheduleRecalculation();
        }

        /// <summary>
        /// Handles grouping changes by recalculating all summaries.
        /// </summary>
        public void OnGroupingChanged()
        {
            if (_disposed) return;

            InvalidateAll();
            ScheduleRecalculation();
        }

        private void RecalculateColumnSummaries(DataGridColumn column, IEnumerable items, DataGridSummaryScope scope, DataGridCollectionViewGroup? group)
        {
            foreach (var description in column.Summaries)
            {
                if (description.AppliesTo(scope))
                {
                    try
                    {
                        var value = description.Calculate(items, column);
                        _cache.Set(column, description, scope, group, value);
                    }
                    catch
                    {
                        // Log or handle calculation errors
                        _cache.Set(column, description, scope, group, null);
                    }
                }
            }
        }

        private IEnumerable GetAllItems()
        {
            var collectionView = _owner.CollectionView;
            if (collectionView != null)
            {
                return collectionView;
            }

            return _owner.ItemsSource ?? Array.Empty<object>();
        }

        private IEnumerable GetGroupItems(DataGridCollectionViewGroup group)
        {
            return GetLeafItems(group);
        }

        private IEnumerable<object> GetLeafItems(DataGridCollectionViewGroup group)
        {
            foreach (var item in group.Items)
            {
                if (item is DataGridCollectionViewGroup subGroup)
                {
                    foreach (var leafItem in GetLeafItems(subGroup))
                    {
                        yield return leafItem;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<DataGridCollectionViewGroup> GetAllGroups()
        {
            var collectionView = _owner.CollectionView;
            if (collectionView == null)
            {
                yield break;
            }

            var groups = collectionView.Groups;
            if (groups == null)
            {
                yield break;
            }

            foreach (var group in groups.OfType<DataGridCollectionViewGroup>())
            {
                yield return group;

                foreach (var subGroup in GetAllSubGroups(group))
                {
                    yield return subGroup;
                }
            }
        }

        private IEnumerable<DataGridCollectionViewGroup> GetAllSubGroups(DataGridCollectionViewGroup group)
        {
            foreach (var item in group.Items)
            {
                if (item is DataGridCollectionViewGroup subGroup)
                {
                    yield return subGroup;

                    foreach (var nested in GetAllSubGroups(subGroup))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private void RaiseSummaryRecalculated(DataGridSummaryScope scope, DataGridCollectionViewGroup? group)
        {
            SummaryRecalculated?.Invoke(this, new DataGridSummaryRecalculatedEventArgs(scope, group));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
                _debounceTimer.Tick -= OnDebounceTimerTick;
                _debounceTimer = null;
            }

            _cache.InvalidateAll();
        }
    }

    /// <summary>
    /// Event arguments for when summaries are recalculated.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    class DataGridSummaryRecalculatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the scope that was recalculated.
        /// </summary>
        public DataGridSummaryScope Scope { get; }

        /// <summary>
        /// Gets the specific group that was recalculated (null for total scope or all groups).
        /// </summary>
        public DataGridCollectionViewGroup? Group { get; }

        public DataGridSummaryRecalculatedEventArgs(DataGridSummaryScope scope, DataGridCollectionViewGroup? group)
        {
            Scope = scope;
            Group = group;
        }
    }
}
