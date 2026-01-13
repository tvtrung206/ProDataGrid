using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ContentControlSwapViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        private ItemViewModel? _selectedItem;
        private bool _isRunning;
        private int _intervalMs = 40;
        private long _swapCount;
        private int _nextIndex;

        public ContentControlSwapViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>
            {
                new ItemViewModel("Alpha", 10),
                new ItemViewModel("Beta", 12),
                new ItemViewModel("Gamma", 8)
            };

            _selectedItem = Items.Count > 0 ? Items[0] : null;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_intervalMs)
            };
            _timer.Tick += (_, __) => SwapSelection();

            StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsRunning);
            SwapOnceCommand = new RelayCommand(_ => SwapSelection());
        }

        public ObservableCollection<ItemViewModel> Items { get; }

        public ItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    _nextIndex = value != null ? Items.IndexOf(value) : -1;
                }
            }
        }

        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand SwapOnceCommand { get; }

        public int IntervalMs
        {
            get => _intervalMs;
            set
            {
                var next = Math.Max(5, value);
                if (SetProperty(ref _intervalMs, next) && _timer.IsEnabled)
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(_intervalMs);
                }
            }
        }

        public long SwapCount
        {
            get => _swapCount;
            private set => SetProperty(ref _swapCount, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    StartCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(RunState));
                }
            }
        }

        public string RunState => IsRunning ? "Running" : "Stopped";

        private void Start()
        {
            if (IsRunning)
            {
                return;
            }

            _timer.Interval = TimeSpan.FromMilliseconds(_intervalMs);
            _timer.Start();
            IsRunning = true;
        }

        private void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            _timer.Stop();
            IsRunning = false;
        }

        private void SwapSelection()
        {
            if (Items.Count == 0)
            {
                return;
            }

            _nextIndex = (_nextIndex + 1) % Items.Count;
            SelectedItem = Items[_nextIndex];
            SwapCount++;
        }

        public sealed class ItemViewModel
        {
            public ItemViewModel(string name, int rowCount)
            {
                Name = name;
                Rows = new ObservableCollection<RowViewModel>();

                for (var i = 1; i <= rowCount; i++)
                {
                    Rows.Add(new RowViewModel($"{name} row {i}", i * 10, $"{name} note {i}"));
                }
            }

            public string Name { get; }

            public ObservableCollection<RowViewModel> Rows { get; }
        }

        public sealed class RowViewModel
        {
            public RowViewModel(string name, int value, string note)
            {
                Name = name;
                Value = value;
                Note = note;
            }

            public string Name { get; }

            public int Value { get; }

            public string Note { get; }
        }
    }
}
