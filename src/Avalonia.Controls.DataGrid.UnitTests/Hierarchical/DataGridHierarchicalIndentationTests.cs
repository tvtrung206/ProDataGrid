// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Hierarchical;

public class DataGridHierarchicalIndentationTests
{
    [AvaloniaFact]
    public void HierarchicalIndentation_Updates_After_Reparent()
    {
        var rootA = new Node("Root A");
        var rootB = new Node("Root B");
        var child = new Node("Child");
        rootA.Children.Add(child);

        var roots = new ObservableCollection<Node> { rootA, rootB };

        var model = new HierarchicalModel<Node>(new HierarchicalOptions<Node>
        {
            ChildrenSelector = node => node.Children,
            IsLeafSelector = node => node.Children.Count == 0,
            AutoExpandRoot = true,
            MaxAutoExpandDepth = 2
        });
        model.SetRoots(roots);
        model.ExpandAll();

        const double indent = 24;
        var (root, grid) = CreateGrid(model, indent);

        try
        {
            var initialRow = FindRowForItem(grid, child);
            var initialPresenter = FindPresenter(initialRow);
            Assert.Equal(new Thickness(indent, 0, 0, 0), initialPresenter.Padding);

            rootA.Children.Remove(child);
            roots.Add(child);
            Dispatcher.UIThread.RunJobs();
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var movedRow = FindRowForItem(grid, child);
            var movedPresenter = FindPresenter(movedRow);
            Assert.Equal(new Thickness(0, 0, 0, 0), movedPresenter.Padding);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void HierarchicalIndentation_Updates_After_Subtree_Reparent_Undo_Redo()
    {
        var rootA = new Node("Root A");
        var rootB = new Node("Root B");
        var parent = new Node("Parent");
        var child = new Node("Child");
        parent.Children.Add(child);
        rootA.Children.Add(parent);

        var roots = new ObservableCollection<Node> { rootA, rootB };

        var model = new HierarchicalModel<Node>(new HierarchicalOptions<Node>
        {
            ChildrenSelector = node => node.Children,
            IsLeafSelector = node => node.Children.Count == 0,
            AutoExpandRoot = true,
            MaxAutoExpandDepth = 2
        });
        model.SetRoots(roots);
        model.ExpandAll();

        const double indent = 24;
        var (root, grid) = CreateGrid(model, indent);

        try
        {
            PumpLayout(grid);
            AssertIndentation(grid, parent, indent, level: 1);
            AssertIndentation(grid, child, indent, level: 2);

            rootA.Children.Remove(parent);
            roots.Add(parent);
            model.ExpandAll();
            PumpLayout(grid);

            AssertIndentation(grid, parent, indent, level: 0);
            AssertIndentation(grid, child, indent, level: 1);

            roots.Remove(parent);
            rootA.Children.Add(parent);
            model.ExpandAll();
            PumpLayout(grid);

            AssertIndentation(grid, parent, indent, level: 1);
            AssertIndentation(grid, child, indent, level: 2);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void HierarchicalIndentation_Updates_After_Resetting_Roots()
    {
        var rootA = new Node("Root A");
        var childA = new Node("Child A");
        rootA.Children.Add(childA);

        var roots = new ObservableCollection<Node> { rootA };

        var model = new HierarchicalModel<Node>(new HierarchicalOptions<Node>
        {
            ChildrenSelector = node => node.Children,
            IsLeafSelector = node => node.Children.Count == 0,
            AutoExpandRoot = true,
            MaxAutoExpandDepth = 1
        });
        model.SetRoots(roots);
        model.ExpandAll();

        const double indent = 24;
        var (root, grid) = CreateGrid(model, indent);

        try
        {
            PumpLayout(grid);
            AssertIndentation(grid, childA, indent, level: 1);

            var rootB = new Node("Root B");
            var childB = new Node("Child B");
            rootB.Children.Add(childB);

            var newRoots = new ObservableCollection<Node> { rootB };
            model.SetRoots(newRoots);
            model.ExpandAll();
            PumpLayout(grid);

            AssertIndentation(grid, childB, indent, level: 1);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void HierarchicalIndentation_Updates_After_Roots_Reset_Via_Collection_Change()
    {
        var rootA = new Node("Root A");
        var childA = new Node("Child A");
        rootA.Children.Add(childA);

        var rootB = new Node("Root B");
        var childB = new Node("Child B");
        rootB.Children.Add(childB);

        var roots = new ObservableCollection<Node> { rootA };

        var model = new HierarchicalModel<Node>(new HierarchicalOptions<Node>
        {
            ChildrenSelector = node => node.Children,
            IsLeafSelector = node => node.Children.Count == 0,
            AutoExpandRoot = true,
            MaxAutoExpandDepth = 1
        });
        model.SetRoots(roots);
        model.ExpandAll();

        const double indent = 24;
        var (root, grid) = CreateGrid(model, indent);

        try
        {
            PumpLayout(grid);
            AssertIndentation(grid, childA, indent, level: 1);

            roots.Clear();
            roots.Add(rootB);
            model.ExpandAll();
            PumpLayout(grid);

            Assert.Null(model.FindNode(childA));
            Assert.NotNull(model.FindNode(childB));
            AssertIndentation(grid, childB, indent, level: 1);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void HierarchicalIndentation_Updates_After_Child_Collection_Reset()
    {
        var rootNode = new Node("Root");
        var childA = new Node("Child A");
        var childB = new Node("Child B");
        rootNode.Children.Add(childA);

        var roots = new ObservableCollection<Node> { rootNode };

        var model = new HierarchicalModel<Node>(new HierarchicalOptions<Node>
        {
            ChildrenSelector = node => node.Children,
            IsLeafSelector = node => node.Children.Count == 0,
            AutoExpandRoot = true,
            MaxAutoExpandDepth = 1
        });
        model.SetRoots(roots);
        model.ExpandAll();

        const double indent = 24;
        var (root, grid) = CreateGrid(model, indent);

        try
        {
            PumpLayout(grid);
            AssertIndentation(grid, childA, indent, level: 1);

            rootNode.Children.Clear();
            rootNode.Children.Add(childB);
            model.ExpandAll();
            PumpLayout(grid);

            Assert.Null(model.FindNode(childA));
            Assert.NotNull(model.FindNode(childB));
            AssertIndentation(grid, childB, indent, level: 1);
        }
        finally
        {
            root.Close();
        }
    }

    private static DataGridRow FindRowForItem(DataGrid grid, Node item)
    {
        var row = grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .FirstOrDefault(candidate => candidate.DataContext is HierarchicalNode node &&
                ReferenceEquals(node.Item, item));

        if (row != null)
        {
            return row;
        }

        var model = grid.HierarchicalModel;
        if (model != null)
        {
            var rowIndex = model.IndexOf(item);
            if (rowIndex >= 0)
            {
                var slot = grid.SlotFromRowIndex(rowIndex);
                if (grid.ColumnDefinitions.Count > 0)
                {
                    var columnIndex = grid.ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? 0;
                    grid.ScrollSlotIntoView(columnIndex, slot, forCurrentCellChange: false, forceHorizontalScroll: false);
                }

                PumpLayout(grid);

                if (slot >= grid.DisplayData.FirstScrollingSlot && slot <= grid.DisplayData.LastScrollingSlot)
                {
                    if (grid.DisplayData.GetDisplayedElement(slot) is DataGridRow displayedRow)
                    {
                        return displayedRow;
                    }
                }
            }
        }

        var nodeItem = model?.FindNode(item);
        var scrollTarget = nodeItem ?? (object)item;

        if (grid.ColumnDefinitions.Count > 0)
        {
            grid.ScrollIntoView(scrollTarget, grid.ColumnDefinitions[0]);
        }

        PumpLayout(grid);

        return grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .First(candidate => candidate.DataContext is HierarchicalNode node &&
                ReferenceEquals(node.Item, item));
    }

    private static DataGridHierarchicalPresenter FindPresenter(DataGridRow row)
    {
        return row.GetVisualDescendants()
            .OfType<DataGridHierarchicalPresenter>()
            .First();
    }

    private static void AssertIndentation(DataGrid grid, Node item, double indent, int level)
    {
        var row = FindRowForItem(grid, item);
        var presenter = FindPresenter(row);
        var expected = new Thickness(indent * level, 0, 0, 0);
        Assert.Equal(expected, presenter.Padding);
    }

    private static (Window root, DataGrid grid) CreateGrid(HierarchicalModel<Node> model, double indent)
    {
        var root = new Window
        {
            Width = 600,
            Height = 400,
        };

        root.SetThemeStyles();

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            HierarchicalRowsEnabled = true,
            HierarchicalModel = model
        };

        grid.ColumnsInternal.Add(new DataGridHierarchicalColumn
        {
            Header = "Name",
            Binding = new Binding("Item.Name"),
            Indent = indent
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        return (root, grid);
    }

    private static void PumpLayout(DataGrid grid)
    {
        Dispatcher.UIThread.RunJobs();
        if (grid.GetVisualRoot() is Window window)
        {
            window.ApplyTemplate();
            window.UpdateLayout();
        }
        grid.ApplyTemplate();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class Node
    {
        public Node(string name)
        {
            Name = name;
            Children = new ObservableCollection<Node>();
        }

        public string Name { get; }

        public ObservableCollection<Node> Children { get; }
    }
}
