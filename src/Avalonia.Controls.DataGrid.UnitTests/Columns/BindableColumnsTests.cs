using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Data;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns
{
    public class BindableColumnsTests
    {
        [Fact]
        public void Binding_Columns_Populates_Grid()
        {
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "First", Binding = new Binding("First") },
                new DataGridCheckBoxColumn { Header = "Active", Binding = new Binding("IsActive") }
            };

            var grid = new DataGrid
            {
                Columns = columns
            };

            Assert.Same(columns, grid.Columns);
            Assert.Contains(columns[0], grid.ColumnsInternal.ItemsInternal);
            Assert.Contains(columns[1], grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_Columns_Add_Reflects_In_Grid()
        {
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "First", Binding = new Binding("First") },
            };

            var grid = new DataGrid
            {
                Columns = columns
            };

            var added = new DataGridTextColumn { Header = "Last", Binding = new Binding("Last") };
            columns.Add(added);

            Assert.Contains(added, grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_List_Applies_Snapshot_When_No_INCC()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var columns = new List<DataGridColumn> { first };

            var grid = new DataGrid
            {
                Columns = columns
            };

            Assert.Contains(first, grid.ColumnsInternal.ItemsInternal);

            var second = new DataGridTextColumn { Header = "Second", Binding = new Binding("Second") };
            columns.Add(second);

            Assert.DoesNotContain(second, grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_Enumerable_Applies_Snapshot_When_Not_IList()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var source = new EnumerableColumnsSource(first);

            var grid = new DataGrid
            {
                Columns = source
            };

            Assert.Contains(first, grid.ColumnsInternal.ItemsInternal);

            var second = new DataGridTextColumn { Header = "Second", Binding = new Binding("Second") };
            source.Add(second);

            Assert.DoesNotContain(second, grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_List_TwoWay_Falls_Back_To_OneWay()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var columns = new List<DataGridColumn> { first };

            var grid = new DataGrid
            {
                ColumnsSynchronizationMode = ColumnsSynchronizationMode.TwoWay,
                Columns = columns
            };

            grid.ColumnsInternal.RemoveAt(0);

            Assert.Single(columns);
            Assert.Same(first, columns[0]);
        }

        [Fact]
        public void Binding_Xaml_Binding_Is_Parsed()
        {
            const string xaml = @"<DataGrid xmlns=""https://github.com/avaloniaui""
                                         Columns=""{Binding Columns}""
                                         ItemsSource=""{Binding Items}""/>";

            var grid = ParseGridFromXaml(xaml);
            var vm = new
            {
                Items = new[] { new { First = "A" } },
                Columns = new ObservableCollection<DataGridColumn>
                {
                    new DataGridTextColumn { Header = "First", Binding = new Binding("First") }
                }
            };

            grid.DataContext = vm;
            grid.ApplyTemplate();

            Assert.Contains(vm.Columns[0], grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_Columns_Remove_Reflects_In_Grid()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var last = new DataGridTextColumn { Header = "Last", Binding = new Binding("Last") };
            var columns = new ObservableCollection<DataGridColumn> { first, last };

            var grid = new DataGrid
            {
                Columns = columns
            };

            columns.RemoveAt(0);

            Assert.Single(grid.ColumnsInternal.ItemsInternal, c => c is not DataGridFillerColumn);
            Assert.DoesNotContain(first, grid.ColumnsInternal.ItemsInternal);
            Assert.Contains(last, grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_Move_Reflects_In_Grid()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var last = new DataGridTextColumn { Header = "Last", Binding = new Binding("Last") };
            var columns = new ObservableCollection<DataGridColumn> { first, last };

            var grid = new DataGrid
            {
                Columns = columns
            };

            columns.Move(1, 0);

            var ordered = grid.ColumnsInternal.GetDisplayedColumns(c => c is not DataGridFillerColumn).ToList();
            Assert.Equal(new[] { "Last", "First" }, ordered.Select(c => c.Header));
        }

        [Fact]
        public void Binding_Replace_Reflects_In_Grid()
        {
            var original = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var replacement = new DataGridTextColumn { Header = "New", Binding = new Binding("New") };
            var columns = new ObservableCollection<DataGridColumn> { original };

            var grid = new DataGrid
            {
                Columns = columns
            };

            columns[0] = replacement;

            Assert.DoesNotContain(original, grid.ColumnsInternal.ItemsInternal);
            Assert.Contains(replacement, grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void TwoWay_Remove_From_Grid_Removes_From_Bound_Collection()
        {
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "First", Binding = new Binding("First") },
                new DataGridTextColumn { Header = "Last", Binding = new Binding("Last") }
            };

            var grid = new DataGrid
            {
                ColumnsSynchronizationMode = ColumnsSynchronizationMode.TwoWay,
                Columns = columns
            };

            grid.ColumnsInternal.RemoveAt(0);

            Assert.Single(columns);
            Assert.Equal("Last", columns[0].Header);
        }

        [Fact]
        public void TwoWay_Move_In_Grid_Reorders_Bound_Collection()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var last = new DataGridTextColumn { Header = "Last", Binding = new Binding("Last") };
            var columns = new ObservableCollection<DataGridColumn> { first, last };

            var grid = new DataGrid
            {
                ColumnsSynchronizationMode = ColumnsSynchronizationMode.TwoWay,
                Columns = columns
            };

            grid.ColumnsInternal.Move(0, 1);

            Assert.Same(last, columns[0]);
            Assert.Same(first, columns[1]);
        }

        [Fact]
        public void TwoWay_Add_In_Grid_Adds_To_Bound_Source()
        {
            var first = new DataGridTextColumn { Header = "First", Binding = new Binding("First") };
            var columns = new ObservableCollection<DataGridColumn> { first };

            var grid = new DataGrid
            {
                ColumnsSynchronizationMode = ColumnsSynchronizationMode.TwoWay,
                Columns = columns
            };

            var added = new DataGridTextColumn { Header = "Added", Binding = new Binding("Value") };
            grid.ColumnsInternal.Insert(0, added);

            Assert.Equal(2, columns.Count);
            Assert.Same(added, columns[0]);
        }

        [Fact]
        public void TwoWay_Does_Not_Push_AutoGenerated_To_Bound_Source()
        {
            var manual = new DataGridTextColumn { Header = "Manual", Binding = new Binding("Value") };
            var columns = new ObservableCollection<DataGridColumn> { manual };

            var grid = CreateMeasuredGrid();
            grid.ColumnsSynchronizationMode = ColumnsSynchronizationMode.TwoWay;
            grid.Columns = columns;
            grid.AutoGenerateColumns = true;
            grid.ItemsSource = new[] { new { Value = 1, Extra = "x" } };

            MarkMeasured(grid);
            InvokeAutoGenerate(grid);

            Assert.Single(columns);
            Assert.Same(manual, columns[0]);
            Assert.Contains(grid.ColumnsInternal.ItemsInternal, c => c.IsAutoGenerated);
        }

        [Fact]
        public void Binding_Throws_When_Inline_Columns_Exist()
        {
            var grid = new DataGrid();
            grid.ColumnsInternal.Add(new DataGridTextColumn { Header = "Inline", Binding = new Binding("Value") });

            var columns = new ObservableCollection<DataGridColumn>();

            Assert.Throws<InvalidOperationException>(() => grid.Columns = columns);
        }

        [Fact]
        public void Binding_Null_Clears_Columns()
        {
            var grid = new DataGrid();
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "Inline", Binding = new Binding("Value") }
            };

            grid.Columns = columns;
            Assert.Single(grid.ColumnsInternal.ItemsInternal);

            grid.Columns = null!;

            Assert.DoesNotContain(grid.ColumnsInternal.ItemsInternal, c => c is not DataGridFillerColumn);
        }

        [Fact]
        public void Binding_Reset_Reloads_When_Behavior_Reload()
        {
            var columns = new TestColumnsCollection
            {
                new DataGridTextColumn { Header = "One", Binding = new Binding("One") },
                new DataGridTextColumn { Header = "Two", Binding = new Binding("Two") }
            };

            var grid = new DataGrid
            {
                ColumnsSourceResetBehavior = ColumnsSourceResetBehavior.Reload,
                Columns = columns
            };

            columns.ClearAndReset(new[]
            {
                new DataGridTextColumn { Header = "Three", Binding = new Binding("Three") }
            });

            Assert.Single(grid.ColumnsInternal.ItemsInternal, c => c is not DataGridFillerColumn);
            Assert.Equal("Three", grid.ColumnsInternal.ItemsInternal.First(c => c is not DataGridFillerColumn).Header);
        }

        [Fact]
        public void Binding_Reset_Ignored_When_Behavior_Ignore()
        {
            var initial = new DataGridTextColumn { Header = "One", Binding = new Binding("One") };
            var columns = new TestColumnsCollection { initial };

            var grid = new DataGrid
            {
                ColumnsSourceResetBehavior = ColumnsSourceResetBehavior.Ignore,
                Columns = columns
            };

            columns.ClearAndReset(new[]
            {
                new DataGridTextColumn { Header = "Two", Binding = new Binding("Two") }
            });

            Assert.Same(initial, grid.ColumnsInternal.ItemsInternal.First(c => c is not DataGridFillerColumn));
        }

        [Fact]
        public void AutoGenerated_Placement_BeforeSource_Inserts_Before()
        {
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "Manual", Binding = new Binding("Value") }
            };

            var grid = CreateMeasuredGrid();
            grid.Columns = columns;
            grid.AutoGenerateColumns = true;
            grid.AutoGeneratedColumnsPlacement = AutoGeneratedColumnsPlacement.BeforeSource;
            grid.ItemsSource = new[] { new { Value = 1, Extra = "x" } };

            MarkMeasured(grid);
            InvokeAutoGenerate(grid);

            var ordered = grid.ColumnsInternal.ItemsInternal.Where(c => c is not DataGridFillerColumn).ToList();
            Assert.True(ordered.Count >= 2, $"Expected at least auto + manual columns. Count={ordered.Count}, Headers={string.Join(",", ordered.Select(c => c.Header))}");
            Assert.True(ordered[0].IsAutoGenerated);
            Assert.Equal("Manual", ordered.Last().Header);
        }

        [Fact]
        public void AutoGenerated_Placement_None_Removes_Generated()
        {
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "Manual", Binding = new Binding("Value") }
            };

            var grid = CreateMeasuredGrid();
            grid.Columns = columns;
            grid.AutoGenerateColumns = true;
            grid.AutoGeneratedColumnsPlacement = AutoGeneratedColumnsPlacement.None;
            grid.ItemsSource = new[] { new { Value = 1, Extra = "x" } };

            MarkMeasured(grid);
            InvokeAutoGenerate(grid);

            var ordered = grid.ColumnsInternal.ItemsInternal.Where(c => c is not DataGridFillerColumn).ToList();
            Assert.Single(ordered);
            Assert.Equal("Manual", ordered[0].Header);
        }

        [Fact]
        public void Binding_Adding_Null_Throws()
        {
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "Valid", Binding = new Binding("Value") }
            };

            var grid = new DataGrid
            {
                Columns = columns
            };

            Assert.Throws<ArgumentNullException>(() => columns.Add(null!));
        }

        [Fact]
        public void Binding_Adding_Column_From_Other_Grid_Throws()
        {
            var other = new DataGrid
            {
                Columns = new ObservableCollection<DataGridColumn>
                {
                    new DataGridTextColumn { Header = "External", Binding = new Binding("Value") }
                }
            };

            var column = other.ColumnsInternal.ItemsInternal.First(c => c is not DataGridFillerColumn);

            var columns = new ObservableCollection<DataGridColumn>();
            var grid = new DataGrid { Columns = columns };

            Assert.Throws<InvalidOperationException>(() => columns.Add(column));
        }

        [Fact]
        public async Task Binding_CrossThread_Change_Throws()
        {
            var tcs = new TaskCompletionSource<Exception?>();
            var columns = new AsyncColumnsCollection(tcs);
            var grid = new DataGrid
            {
                Columns = columns
            };

            columns.Add(new DataGridTextColumn { Header = "First", Binding = new Binding("First") });

            var ex = await tcs.Task;
            Assert.IsType<InvalidOperationException>(ex);
            Assert.DoesNotContain(columns[0], grid.ColumnsInternal.ItemsInternal);
        }

        [Fact]
        public void Binding_Enumeration_Exception_Leaves_Grid_Intact()
        {
            var grid = new DataGrid();
            var throwing = new ThrowingColumnsList();

            Assert.Throws<InvalidOperationException>(() => grid.Columns = throwing);
            Assert.DoesNotContain(grid.ColumnsInternal.ItemsInternal, c => c is DataGridTextColumn);
        }

        [Fact]
        public void Binding_Pending_During_AutoGen_Reapplies()
        {
            var grid = CreateMeasuredGrid();
            var columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn { Header = "First", Binding = new Binding("First") }
            };

            typeof(DataGrid)
                .GetField("_autoGeneratingColumnOperationCount", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(grid, (byte)1);

            grid.Columns = columns;
            Assert.DoesNotContain(columns[0], grid.ColumnsInternal.ItemsInternal);

            typeof(DataGrid)
                .GetField("_autoGeneratingColumnOperationCount", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(grid, (byte)0);

            MarkMeasured(grid);
            InvokeAutoGenerate(grid);

            Assert.Contains(columns[0], grid.ColumnsInternal.ItemsInternal);
        }

        private static DataGrid CreateMeasuredGrid()
        {
            var grid = new DataGrid();
            typeof(DataGrid)
                .GetField("_measured", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(grid, true);
            return grid;
        }

        private static void InvokeAutoGenerate(DataGrid grid)
        {
            typeof(DataGrid)
                .GetMethod("AutoGenerateColumnsPrivate", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(grid, null);
        }

        private static void MarkMeasured(DataGrid grid)
        {
            typeof(DataGrid)
                .GetField("_measured", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(grid, true);
        }

        private static DataGrid ParseGridFromXaml(string xaml) =>
            AvaloniaRuntimeXamlLoader.Parse<DataGrid>(xaml, typeof(DataGrid).Assembly);

        private sealed class TestColumnsCollection : ObservableCollection<DataGridColumn>
        {
            public void ClearAndReset(DataGridColumn[] replacement)
            {
                Items.Clear();
                foreach (var c in replacement)
                {
                    Items.Add(c);
                }
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private sealed class AsyncColumnsCollection : ObservableCollection<DataGridColumn>
        {
            private readonly TaskCompletionSource<Exception?> _completion;

            public AsyncColumnsCollection(TaskCompletionSource<Exception?> completion)
            {
                _completion = completion;
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                Task.Run(() =>
                {
                    try
                    {
                        base.OnCollectionChanged(e);
                        _completion.TrySetResult(null);
                    }
                    catch (Exception ex)
                    {
                        _completion.TrySetResult(ex);
                        throw;
                    }
                });
            }
        }

        private sealed class EnumerableColumnsSource : IEnumerable<DataGridColumn>
        {
            private readonly List<DataGridColumn> _inner;

            public EnumerableColumnsSource(params DataGridColumn[] columns)
            {
                _inner = new List<DataGridColumn>(columns);
            }

            public void Add(DataGridColumn column) => _inner.Add(column);

            public IEnumerator<DataGridColumn> GetEnumerator() => _inner.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ThrowingColumnsList : IList<DataGridColumn>
        {
            private readonly List<DataGridColumn> _inner = new();

            public DataGridColumn this[int index]
            {
                get => _inner[index];
                set => _inner[index] = value;
            }

            public int Count => _inner.Count;

            public bool IsReadOnly => false;

            public void Add(DataGridColumn item) => _inner.Add(item);

            public void Clear() => _inner.Clear();

            public bool Contains(DataGridColumn item) => _inner.Contains(item);

            public void CopyTo(DataGridColumn[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

            public IEnumerator<DataGridColumn> GetEnumerator() => throw new InvalidOperationException("Enumerating columns failed.");

            public int IndexOf(DataGridColumn item) => _inner.IndexOf(item);

            public void Insert(int index, DataGridColumn item) => _inner.Insert(index, item);

            public bool Remove(DataGridColumn item) => _inner.Remove(item);

            public void RemoveAt(int index) => _inner.RemoveAt(index);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
