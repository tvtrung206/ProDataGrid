// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.DataGridDragDrop;

namespace Avalonia.Controls.DataGridDragDrop
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridRowDropEventArgs
    {
#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        DataGridRowDropEventArgs(
            DataGrid grid,
            IList? targetList,
            IReadOnlyList<object> items,
            IReadOnlyList<int> sourceIndices,
            object? targetItem,
            int targetIndex,
            int insertIndex,
            DataGridRow? targetRow,
            DataGridRowDropPosition position,
            bool isSameGrid,
            DragDropEffects requestedEffect,
            DragEventArgs dragEventArgs)
        {
            Grid = grid;
            TargetList = targetList;
            Items = items;
            SourceIndices = sourceIndices;
            TargetItem = targetItem;
            TargetIndex = targetIndex;
            InsertIndex = insertIndex;
            TargetRow = targetRow;
            Position = position;
            IsSameGrid = isSameGrid;
            RequestedEffect = requestedEffect;
            DragEventArgs = dragEventArgs;
        }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        DataGrid Grid { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        IList? TargetList { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        IReadOnlyList<object> Items { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        IReadOnlyList<int> SourceIndices { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        object? TargetItem { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        int TargetIndex { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        int InsertIndex { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        DataGridRow? TargetRow { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        DataGridRowDropPosition Position { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        bool IsSameGrid { get; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        DragDropEffects RequestedEffect { get; set; }

#if !DATAGRID_INTERNAL
        public
#else
        internal
#endif
        DragEventArgs DragEventArgs { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IDataGridRowDropHandler
    {
        bool Validate(DataGridRowDropEventArgs args);

        bool Execute(DataGridRowDropEventArgs args);
    }
}
