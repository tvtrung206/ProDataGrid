// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Reflection;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Sorting;

public class DataGridSortingAdapterTests
{
    [AvaloniaFact]
    public void Model_Changes_Apply_To_View_SortDescriptions()
    {
        var column = new DataGridTextColumn { SortMemberPath = "Name" };
        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        model.Toggle(new SortingDescriptor(column, ListSortDirection.Ascending, column.GetSortPropertyName(), culture: view.Culture));

        var sort = Assert.Single(view.SortDescriptions);
        Assert.Equal("Name", sort.PropertyPath);
        Assert.Equal(ListSortDirection.Ascending, sort.Direction);
    }

    [AvaloniaFact]
    public void HandleHeaderClick_Supports_Multi_Sort_With_Shift()
    {
        var first = new DataGridTextColumn { SortMemberPath = "Name" };
        var second = new DataGridTextColumn { SortMemberPath = "Age" };
        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => new[] { first, second });
        adapter.AttachView(view);

        adapter.HandleHeaderClick(first, KeyModifiers.None);
        adapter.HandleHeaderClick(second, KeyModifiers.Shift);

        Assert.Equal(new object[] { first, second }, model.Descriptors.Select(x => x.ColumnId).ToArray());
        Assert.Equal(new[] { "Name", "Age" }, view.SortDescriptions.Select(s => s.PropertyPath).ToArray());
    }

    [AvaloniaFact]
    public void HandleHeaderClick_Ctrl_Clears_Sort()
    {
        var column = new DataGridTextColumn { SortMemberPath = "Name" };
        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        adapter.HandleHeaderClick(column, KeyModifiers.None);
        adapter.HandleHeaderClick(column, KeyModifiers.Control);

        Assert.Empty(model.Descriptors);
        Assert.Empty(view.SortDescriptions);
    }

    [AvaloniaFact]
    public void External_View_Sorts_Synchronize_When_Model_Does_Not_Own()
    {
        var column = new DataGridTextColumn { SortMemberPath = "Name" };
        var view = CreateView();
        var model = new SortingModel
        {
            OwnsViewSorts = false
        };
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        view.SortDescriptions.Add(DataGridSortDescription.FromPath("Name", ListSortDirection.Descending));

        var descriptor = Assert.Single(model.Descriptors);
        Assert.Equal(column, descriptor.ColumnId);
        Assert.Equal(ListSortDirection.Descending, descriptor.Direction);
    }

    [AvaloniaFact]
    public void External_View_Sorts_With_Duplicates_Are_Deduped()
    {
        var column = new DataGridTextColumn { SortMemberPath = "Name" };
        var view = CreateView();
        view.SortDescriptions.Add(DataGridSortDescription.FromPath("Name", ListSortDirection.Ascending));
        view.SortDescriptions.Add(DataGridSortDescription.FromPath("Name", ListSortDirection.Descending));

        var model = new SortingModel { OwnsViewSorts = false };
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        var descriptor = Assert.Single(model.Descriptors);
        Assert.Equal(ListSortDirection.Ascending, descriptor.Direction);
    }

    [AvaloniaFact]
    public void HandleHeaderClick_No_Path_Does_Not_Add_Descriptor()
    {
        var column = new DataGridTextColumn();
        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        adapter.HandleHeaderClick(column, KeyModifiers.None);

        Assert.Empty(model.Descriptors);
        Assert.Empty(view.SortDescriptions);
    }

    [AvaloniaFact]
    public void HandleHeaderClick_Uses_Definition_Id_When_Available()
    {
        var definition = new DataGridTextColumnDefinition
        {
            Header = "Name",
            SortMemberPath = "Name"
        };

        var grid = new DataGrid
        {
            ColumnDefinitionsSource = new[] { definition }
        };

        var column = grid.ColumnsInternal.First(c => c is DataGridTextColumn);
        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => grid.Columns);
        adapter.AttachView(view);

        adapter.HandleHeaderClick(column, KeyModifiers.None);

        var descriptor = Assert.Single(model.Descriptors);
        Assert.Same(definition, descriptor.ColumnId);
    }

    [AvaloniaFact]
    public void HandleHeaderClick_Adds_Comparer_Sort()
    {
        var comparer = Comparer<object>.Create((x, y) =>
            Comparer<int>.Default.Compare(((Person)x).Age, ((Person)y).Age));
        var column = new DataGridTextColumn { SortMemberPath = "Value", CustomSortComparer = comparer };
        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        adapter.HandleHeaderClick(column, KeyModifiers.None);

        var sort = Assert.IsType<DataGridComparerSortDescription>(Assert.Single(view.SortDescriptions));
        Assert.Same(comparer, sort.SourceComparer);
        var descriptor = Assert.Single(model.Descriptors);
        Assert.True(descriptor.HasComparer);
        Assert.Equal(column, descriptor.ColumnId);
    }

    [AvaloniaFact]
    public void HandleHeaderClick_Uses_ValueAccessor_When_No_Path()
    {
        var column = new DataGridTextColumn();
        var accessor = new DataGridColumnValueAccessor<Person, int>(p => p.Age);
        DataGridColumnMetadata.SetValueAccessor(column, accessor);

        var view = CreateView();
        var model = new SortingModel();
        var adapter = new DataGridSortingAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        adapter.HandleHeaderClick(column, KeyModifiers.None);

        var sort = Assert.IsType<DataGridComparerSortDescription>(Assert.Single(view.SortDescriptions));
        Assert.IsType<DataGridColumnValueAccessorComparer>(sort.SourceComparer);
        Assert.Single(model.Descriptors);
        Assert.True(model.Descriptors[0].HasComparer);
    }

    [AvaloniaFact]
    public void Model_Descriptor_Uses_Definition_Id_For_ValueAccessor_Sort()
    {
        var items = new[]
        {
            new Person("Alpha", 3),
            new Person("Beta", 1),
            new Person("Gamma", 2)
        };

        var view = new DataGridCollectionView(items);
        var model = new SortingModel();

        var definition = new DataGridTextColumnDefinition
        {
            Header = "Age",
            Binding = DataGridBindingDefinition.Create<Person, int>(p => p.Age)
        };

        var grid = new DataGrid
        {
            ColumnDefinitionsSource = new[] { definition }
        };

        var adapter = new DataGridSortingAdapter(model, () => grid.Columns);
        adapter.AttachView(view);

        model.Toggle(new SortingDescriptor(definition, ListSortDirection.Ascending));

        Assert.Single(view.SortDescriptions);
        var descriptor = Assert.Single(model.Descriptors);
        Assert.Same(definition, descriptor.ColumnId);
    }

    [AvaloniaFact]
    public void Definition_ValueAccessor_Sort_Uses_Descriptor_Culture()
    {
        var items = new[]
        {
            new Person("i", 1),
            new Person("I", 2)
        };

        var view = new DataGridCollectionView(items);
        var model = new SortingModel();

        var definition = new DataGridTextColumnDefinition
        {
            Header = "Name",
            Binding = DataGridBindingDefinition.Create<Person, string>(p => p.Name)
        };

        var grid = new DataGrid
        {
            ColumnDefinitionsSource = new[] { definition }
        };

        var adapter = new DataGridSortingAdapter(model, () => grid.Columns);
        adapter.AttachView(view);

        var culture = new CultureInfo("tr-TR");
        model.Toggle(new SortingDescriptor(definition, ListSortDirection.Ascending, culture: culture));

        var sort = Assert.IsType<DataGridComparerSortDescription>(Assert.Single(view.SortDescriptions));
        var comparer = Assert.IsType<DataGridColumnValueAccessorComparer>(sort.SourceComparer);
        var field = typeof(DataGridColumnValueAccessorComparer).GetField("_culture", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field);
        Assert.Equal(culture, field.GetValue(comparer));
    }

    [AvaloniaFact]
    public void Definition_With_Path_Uses_ValueAccessor_Sort()
    {
        var items = new[]
        {
            new Person("Alpha", 2),
            new Person("Beta", 1)
        };

        var view = new DataGridCollectionView(items);
        var model = new SortingModel();

        var definition = new DataGridTextColumnDefinition
        {
            Header = "Age",
            SortMemberPath = nameof(Person.Age),
            Binding = DataGridBindingDefinition.Create<Person, int>(p => p.Age)
        };

        var grid = new DataGrid
        {
            ColumnDefinitionsSource = new[] { definition }
        };

        var adapter = new DataGridSortingAdapter(model, () => grid.Columns);
        adapter.AttachView(view);

        model.Toggle(new SortingDescriptor(definition, ListSortDirection.Ascending, nameof(Person.Age)));

        var sort = Assert.IsType<DataGridComparerSortDescription>(Assert.Single(view.SortDescriptions));
        Assert.IsType<DataGridColumnValueAccessorComparer>(sort.SourceComparer);
        Assert.Equal(nameof(Person.Age), sort.PropertyPath);
    }

    [AvaloniaFact]
    public void Observe_Mode_Syncs_Grouped_Paged_MultiSort_With_Culture_And_Comparer()
    {
        var culture = new CultureInfo("pl-PL");
        var comparer = Comparer<object>.Create((x, y) =>
            Comparer<int>.Default.Compare(((GroupedPerson)x).Value, ((GroupedPerson)y).Value));

        var view = new DataGridCollectionView(new List<GroupedPerson>
        {
            new("ą", "G1", 2),
            new("a", "G2", 1),
            new("b", "G2", 3),
            new("c", "G1", 1)
        })
        {
            Culture = culture,
            PageSize = 2
        };
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(GroupedPerson.Group)));
        view.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(GroupedPerson.Group), ListSortDirection.Descending, culture));
        view.SortDescriptions.Add(DataGridSortDescription.FromComparer(comparer, ListSortDirection.Ascending));
        view.Refresh();
        view.MoveToFirstPage();

        var groupColumn = new DataGridTextColumn { SortMemberPath = nameof(GroupedPerson.Group) };
        var valueColumn = new DataGridTextColumn { SortMemberPath = nameof(GroupedPerson.Value), CustomSortComparer = comparer };
        var model = new SortingModel { OwnsViewSorts = false };
        var adapter = new DataGridSortingAdapter(model, () => new[] { groupColumn, valueColumn });
        adapter.AttachView(view);

        Assert.Equal(2, model.Descriptors.Count);
        var groupDescriptor = model.Descriptors[0];
        Assert.Equal(nameof(GroupedPerson.Group), groupDescriptor.PropertyPath);
        Assert.Equal(culture, groupDescriptor.Culture);
        Assert.Equal(ListSortDirection.Descending, groupDescriptor.Direction);

        var valueDescriptor = model.Descriptors[1];
        Assert.Same(comparer, valueDescriptor.Comparer);
        Assert.Equal(ListSortDirection.Ascending, valueDescriptor.Direction);

        var firstPage = view.Cast<GroupedPerson>().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "a", "b" }, firstPage);

        Assert.True(view.MoveToNextPage());
        var secondPage = view.Cast<GroupedPerson>().Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "c", "ą" }, secondPage);
    }

    private static DataGridCollectionView CreateView()
    {
        return new DataGridCollectionView(new List<Person>
        {
            new Person("A", 1),
            new Person("B", 2),
            new Person("C", 3)
        });
    }

    private class Person
    {
        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; }

        public int Age { get; }
    }

    private class GroupedPerson
    {
        public GroupedPerson(string name, string group, int value)
        {
            Name = name;
            Group = group;
            Value = value;
        }

        public string Name { get; }

        public string Group { get; }

        public int Value { get; }
    }
}
