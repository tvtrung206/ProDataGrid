// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Avalonia.Controls
{
    /// <summary>
    /// Specifies the scope of a column summary calculation.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    enum DataGridSummaryScope
    {
        /// <summary>
        /// Summary is calculated for all visible/filtered items (grid-level total).
        /// </summary>
        Total,

        /// <summary>
        /// Summary is calculated for items within each group (group-level subtotal).
        /// </summary>
        Group,

        /// <summary>
        /// Summary is displayed at both total and group levels.
        /// </summary>
        Both
    }

    /// <summary>
    /// Specifies the type of aggregate function for a column summary.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    enum DataGridAggregateType
    {
        /// <summary>
        /// No aggregation - displays empty or custom text.
        /// </summary>
        None,

        /// <summary>
        /// Sum of all numeric values.
        /// </summary>
        Sum,

        /// <summary>
        /// Arithmetic mean of all numeric values.
        /// </summary>
        Average,

        /// <summary>
        /// Count of all items.
        /// </summary>
        Count,

        /// <summary>
        /// Count of unique values.
        /// </summary>
        CountDistinct,

        /// <summary>
        /// Minimum value.
        /// </summary>
        Min,

        /// <summary>
        /// Maximum value.
        /// </summary>
        Max,

        /// <summary>
        /// First value in the collection.
        /// </summary>
        First,

        /// <summary>
        /// Last value in the collection.
        /// </summary>
        Last,

        /// <summary>
        /// Custom calculation using a user-provided calculator.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Specifies the position of the total summary row.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    enum DataGridSummaryRowPosition
    {
        /// <summary>
        /// Total summary row appears at the top of the grid.
        /// </summary>
        Top,

        /// <summary>
        /// Total summary row appears at the bottom of the grid.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Specifies the position of group summary rows.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    enum DataGridGroupSummaryPosition
    {
        /// <summary>
        /// Group summary appears in the group header.
        /// </summary>
        Header,

        /// <summary>
        /// Group summary appears as a footer after group items.
        /// </summary>
        Footer,

        /// <summary>
        /// Group summary appears in both header and footer.
        /// </summary>
        Both
    }
}
