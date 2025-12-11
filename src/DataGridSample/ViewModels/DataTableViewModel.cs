using System;
using System.Data;
using System.Linq;
using System.Windows.Input;
using DataGridSample.Mvvm;
using Avalonia.Collections;

namespace DataGridSample.ViewModels
{
    public class DataTableViewModel : ObservableObject
    {
        private DataTable _table;
        private DataRowView? _selectedRow;

        public DataTableViewModel()
        {
            Items = new DataGridCollectionView(BuildTable().DefaultView);

            AddRowCommand = new RelayCommand(_ => AddRow());
            RemoveSelectedCommand = new RelayCommand(_ => RemoveSelected(), _ => SelectedRow != null);
            ResetCommand = new RelayCommand(_ => Reset());
        }

        public DataGridCollectionView Items { get; private set; }

        public DataRowView? SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (SetProperty(ref _selectedRow, value))
                {
                    (RemoveSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddRowCommand { get; }

        public ICommand RemoveSelectedCommand { get; }

        public ICommand ResetCommand { get; }

        private void Reset()
        {
            Items = new DataGridCollectionView(BuildTable().DefaultView);
            SelectedRow = null;
            OnPropertyChanged(nameof(Items));
        }

        private void AddRow()
        {
            if (_table == null)
            {
                return;
            }

            var random = new Random();
            var id = _table.Rows.Count == 0
                ? 1
                : _table.AsEnumerable().Select(r => r.Field<int>("Id")).DefaultIfEmpty().Max() + 1;

            var row = _table.NewRow();
            row["Id"] = id;
            row["Name"] = $"Row {id}";
            row["Balance"] = Math.Round(random.NextDouble() * 5000 - 2500, 2);
            row["Created"] = DateTimeOffset.Now.AddMinutes(-id);
            row["IsActive"] = random.NextDouble() > 0.2;

            _table.Rows.Add(row);

            // DataGridCollectionView will pick up IBindingList changes.
        }

        private void RemoveSelected()
        {
            if (SelectedRow == null)
            {
                return;
            }

            SelectedRow.Delete();
            SelectedRow = null;
        }

        private DataTable BuildTable()
        {
            _table = new DataTable("Sample");

            _table.Columns.Add(new DataColumn("Id", typeof(int)));
            _table.Columns.Add(new DataColumn("Name", typeof(string)));
            _table.Columns.Add(new DataColumn("Balance", typeof(decimal)));
            _table.Columns.Add(new DataColumn("Created", typeof(DateTimeOffset)));
            _table.Columns.Add(new DataColumn("IsActive", typeof(bool)));

            for (var i = 1; i <= 8; i++)
            {
                var row = _table.NewRow();
                row["Id"] = i;
                row["Name"] = $"Row {i}";
                row["Balance"] = Math.Round((i * 137.42m) % 5000 - 1200, 2);
                row["Created"] = DateTimeOffset.Now.AddHours(-i * 3);
                row["IsActive"] = i % 3 != 0;
                _table.Rows.Add(row);
            }

            return _table;
        }
    }
}
