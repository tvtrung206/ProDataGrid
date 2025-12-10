// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Avalonia.Controls.DataGridHierarchical
{
    /// <summary>
    /// Options controlling hierarchical data resolution and behavior.
    /// </summary>
    public class HierarchicalOptions
    {
        /// <summary>
        /// Delegate used to resolve children for a given item.
        /// </summary>
        public Func<object, IEnumerable?>? ChildrenSelector { get; set; }

        /// <summary>
        /// Property path used to fetch children when <see cref="ChildrenSelector"/> is not supplied.
        /// </summary>
        public string? ChildrenPropertyPath { get; set; }

        /// <summary>
        /// Whether the root node should automatically start expanded.
        /// </summary>
        public bool AutoExpandRoot { get; set; }

        /// <summary>
        /// Optional depth limit for automatic expansion.
        /// </summary>
        public int? MaxAutoExpandDepth { get; set; }

        /// <summary>
        /// Whether children should be virtualized (lazy materialization).
        /// </summary>
        public bool VirtualizeChildren { get; set; } = true;

        /// <summary>
        /// Optional comparer applied to siblings when ordering is required.
        /// </summary>
        public IComparer<object>? SiblingComparer { get; set; }

        /// <summary>
        /// Optional delegate that determines whether an item should be treated as a leaf before
        /// children are materialized. Return true for leaf nodes, false for containers. When null,
        /// the model will defer the decision until children are resolved.
        /// </summary>
        public Func<object, bool>? IsLeafSelector { get; set; }

        /// <summary>
        /// Delegate mirroring Avalonia's <c>IHierarchicalDataTemplate.ItemsSelector</c> pattern. Invoked
        /// when <see cref="ChildrenSelector"/> is null.
        /// </summary>
        public Func<object, IEnumerable?>? ItemsSelector { get; set; }

        /// <summary>
        /// Optional depth guard (root is level 0). When set, nodes at or beyond this level are treated as leaves.
        /// </summary>
        public int? MaxDepth { get; set; }

        /// <summary>
        /// When true, grouping rows may be projected as hierarchical nodes (future integration path).
        /// Default is false to keep grouping separate.
        /// </summary>
        public bool TreatGroupsAsNodes { get; set; }
    }
}
