using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class SelectionOriginViewModel : ObservableObject
    {
        private ObservableCollection<Country> _items;
        private string _lastEvent = "Interact with the grid to see selection origin metadata.";
        private Country? _selectedCountry;
        private int _programmaticIndex;

        public SelectionOriginViewModel()
        {
            _items = new ObservableCollection<Country>(Countries.All.Take(18).ToList());
            SelectionModel = new SelectionModel<Country> { SingleSelect = false };
            BoundSelectedItems = new ObservableCollection<object>();
            EventLog = new ObservableCollection<string>();

            ProgrammaticSelectCommand = new RelayCommand(_ => ProgrammaticSelect());
            ReplaceItemsSourceCommand = new RelayCommand(_ => ReplaceItems());
            SelectViaModelCommand = new RelayCommand(_ => SelectViaModel());
            ClearBoundSelectionCommand = new RelayCommand(_ => BoundSelectedItems.Clear());
        }

        public ObservableCollection<Country> Items
        {
            get => _items;
            private set => SetProperty(ref _items, value);
        }

        public SelectionModel<Country> SelectionModel { get; }

        public ObservableCollection<object> BoundSelectedItems { get; }

        public ObservableCollection<string> EventLog { get; }

        public string LastEvent
        {
            get => _lastEvent;
            private set => SetProperty(ref _lastEvent, value);
        }

        public Country? SelectedCountry
        {
            get => _selectedCountry;
            set => SetProperty(ref _selectedCountry, value);
        }

        public RelayCommand ProgrammaticSelectCommand { get; }

        public RelayCommand ReplaceItemsSourceCommand { get; }

        public RelayCommand SelectViaModelCommand { get; }

        public RelayCommand ClearBoundSelectionCommand { get; }

        public void RecordSelectionChange(DataGridSelectionChangedEventArgs e)
        {
            var added = string.Join(", ", e.AddedItems.Cast<object>().Select(Describe).Take(3));
            var removed = string.Join(", ", e.RemovedItems.Cast<object>().Select(Describe).Take(3));
            var trigger = e.TriggerEvent?.GetType().Name ?? "none";

            var message = $"{e.Source} | user: {e.IsUserInitiated} | added: {added} | removed: {removed} | trigger: {trigger}";
            EventLog.Insert(0, message);
            while (EventLog.Count > 40)
            {
                EventLog.RemoveAt(EventLog.Count - 1);
            }

            LastEvent = message;
        }

        private void ProgrammaticSelect()
        {
            if (Items.Count == 0)
            {
                return;
            }

            _programmaticIndex = (_programmaticIndex + 1) % Items.Count;
            SelectedCountry = Items[_programmaticIndex];
        }

        private void ReplaceItems()
        {
            var next = Items == null || Items.FirstOrDefault()?.Region != "North America"
                ? Countries.All.Where(x => x.Region == "North America").Take(8)
                : Countries.All.Where(x => x.Region == "Asia").Take(8);

            Items = new ObservableCollection<Country>(next.ToList());
            _programmaticIndex = -1;
        }

        private void SelectViaModel()
        {
            if (Items.Count == 0)
            {
                return;
            }

            SelectionModel.Clear();
            SelectionModel.Select(0);
            if (Items.Count > 2)
            {
                SelectionModel.SelectRange(0, 2);
            }
        }

        private static string Describe(object? item)
        {
            if (item is Country country)
            {
                return country.Name;
            }

            return item?.ToString() ?? "(null)";
        }
    }
}
