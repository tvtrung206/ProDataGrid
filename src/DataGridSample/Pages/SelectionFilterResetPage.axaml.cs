using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DataGridSample.Models;

namespace DataGridSample.Pages
{
    public partial class SelectionFilterResetPage : UserControl
    {
        private readonly ObservableCollection<SelectionFilterItem> _items;
        private readonly DataGridCollectionView _view;

        public SelectionFilterResetPage()
        {
            InitializeComponent();

            _items = new ObservableCollection<SelectionFilterItem>(
                Enumerable.Range(0, 8).Select(i => new SelectionFilterItem
                {
                    Name = $"Item {i}",
                    Group = i % 2 == 0 ? "A" : "B",
                    Value = i
                }));

            _view = new DataGridCollectionView(_items);
            Grid.ItemsSource = _view;
            Grid.Selection = new SelectionModel<object> { SingleSelect = false };
        }

        private void OnSelectLastRow(object? sender, RoutedEventArgs e)
        {
            if (_view.Count == 0)
            {
                return;
            }

            Grid.SelectedIndex = _view.Count - 1;
        }

        private void OnFilterOutSelected(object? sender, RoutedEventArgs e)
        {
            var selected = Grid.SelectedItem as SelectionFilterItem;
            if (selected == null)
            {
                _view.Filter = null;
                return;
            }

            _view.Filter = item => !ReferenceEquals(item, selected);
        }

        private void OnClearFilter(object? sender, RoutedEventArgs e)
        {
            _view.Filter = null;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Grid = this.FindControl<DataGrid>("Grid");
        }

    }
}
