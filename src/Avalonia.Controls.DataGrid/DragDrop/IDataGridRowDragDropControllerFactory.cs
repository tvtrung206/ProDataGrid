// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Avalonia.Controls.DataGridDragDrop
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IDataGridRowDragDropController : System.IDisposable
    {
        DataGrid Grid { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IDataGridRowDragDropControllerFactory
    {
        IDataGridRowDragDropController Create(
            DataGrid grid,
            IDataGridRowDropHandler dropHandler,
            DataGridRowDragDropOptions options);
    }
}
