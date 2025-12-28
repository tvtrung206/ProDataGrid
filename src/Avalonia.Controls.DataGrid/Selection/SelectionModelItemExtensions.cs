// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections;
using Avalonia.Controls.DataGridHierarchical;

namespace Avalonia.Controls.Selection
{
    #if !DATAGRID_INTERNAL
    public
    #else
    internal
    #endif
    static class SelectionModelItemExtensions
    {
        public static void Select(this ISelectionModel model, object item)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Source == null)
            {
                model.SelectedItem = item;
                return;
            }

            var index = ResolveIndex(model.Source, item);
            if (index < 0)
            {
                throw new ArgumentException("Item not found in selection model source.", nameof(item));
            }

            if (model.SingleSelect)
            {
                model.SelectedIndex = index;
            }
            else
            {
                model.Select(index);
            }
        }

        private static int ResolveIndex(IEnumerable source, object item)
        {
            if (source == null)
            {
                return -1;
            }

            if (source is IList list)
            {
                var index = list.IndexOf(item);
                if (index >= 0)
                {
                    return index;
                }

                for (var i = 0; i < list.Count; i++)
                {
                    if (MatchesHierarchicalItem(list[i], item))
                    {
                        return i;
                    }
                }

                return -1;
            }

            var currentIndex = 0;
            foreach (var entry in source)
            {
                if (Equals(entry, item) || MatchesHierarchicalItem(entry, item))
                {
                    return currentIndex;
                }

                currentIndex++;
            }

            return -1;
        }

        private static bool MatchesHierarchicalItem(object candidate, object item)
        {
            if (candidate is IHierarchicalNodeItem node)
            {
                return Equals(node.Item, item);
            }

            return false;
        }
    }
}
