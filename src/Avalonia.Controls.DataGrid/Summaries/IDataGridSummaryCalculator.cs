// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;

namespace Avalonia.Controls
{
    /// <summary>
    /// Interface for implementing custom summary calculations.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    interface IDataGridSummaryCalculator
    {
        /// <summary>
        /// Gets a name identifying this calculator.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculates the summary value for the given items.
        /// </summary>
        /// <param name="items">The items to summarize.</param>
        /// <param name="column">The column being summarized.</param>
        /// <param name="propertyName">The property name to extract values from.</param>
        /// <returns>The calculated summary value.</returns>
        object? Calculate(IEnumerable items, DataGridColumn column, string? propertyName);

        /// <summary>
        /// Gets whether this calculator supports incremental updates.
        /// </summary>
        bool SupportsIncremental { get; }

        /// <summary>
        /// Creates a new incremental state for accumulating values.
        /// </summary>
        /// <returns>A new state object, or null if incremental calculation is not supported.</returns>
        IDataGridSummaryState? CreateState();
    }

    /// <summary>
    /// State object for incremental summary calculations.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    interface IDataGridSummaryState
    {
        /// <summary>
        /// Resets the state to initial values.
        /// </summary>
        void Reset();

        /// <summary>
        /// Adds a value to the accumulation.
        /// </summary>
        /// <param name="value">The value to add.</param>
        void Add(object? value);

        /// <summary>
        /// Removes a value from the accumulation.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        void Remove(object? value);

        /// <summary>
        /// Gets the current calculated result.
        /// </summary>
        /// <returns>The accumulated result.</returns>
        object? GetResult();
    }
}
