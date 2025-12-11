// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSorting;

namespace DataGridSample.Adapters
{
    /// <summary>
    /// Sorting adapter that short-circuits view sorting for the hierarchical sample.
    /// It allows column header gestures to toggle the sorting model (for glyphs/state)
    /// while preventing the underlying view from being re-ordered.
    /// </summary>
    public sealed class HierarchicalSortingAdapter : DataGridSortingAdapter
    {
        public HierarchicalSortingAdapter(
            ISortingModel model,
            Func<IEnumerable<DataGridColumn>> columnProvider,
            Action beforeViewRefresh,
            Action afterViewRefresh)
            : base(model, columnProvider, beforeViewRefresh, afterViewRefresh)
        {
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<SortingDescriptor> descriptors,
            IReadOnlyList<SortingDescriptor> previousDescriptors,
            out bool changed)
        {
            // Prevent the view from re-sorting the flattened nodes; the hierarchical model
            // handles ordering. Still report a change so the grid refreshes row/column state.
            changed = true;
            return true;
        }
    }

    public sealed class HierarchicalSortingAdapterFactory : IDataGridSortingAdapterFactory
    {
        public DataGridSortingAdapter Create(DataGrid grid, ISortingModel model)
        {
            return new HierarchicalSortingAdapter(
                model,
                () => grid.Columns,
                null,
                null);
        }
    }
}
