// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;

namespace Avalonia.Controls.DataGridFiltering
{
    /// <summary>
    /// Factory for creating a <see cref="DataGridFilteringAdapter"/> without subclassing <see cref="DataGrid"/>.
    /// </summary>
    public interface IDataGridFilteringAdapterFactory
    {
        /// <summary>
        /// Creates a filtering adapter for the given grid and model.
        /// </summary>
        /// <param name="grid">Owning grid.</param>
        /// <param name="model">Filtering model to bridge.</param>
        /// <returns>Adapter instance.</returns>
        DataGridFilteringAdapter Create(DataGrid grid, IFilteringModel model);
    }
}
