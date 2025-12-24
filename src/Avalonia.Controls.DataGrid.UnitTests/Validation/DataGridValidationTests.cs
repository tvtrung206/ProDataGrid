// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.DataGridTests;

public class DataGridValidationTests
{
    [AvaloniaFact]
    public void Invalid_edit_marks_grid_row_and_cell()
    {
        var (grid, root, item, column) = CreateTextValidationGrid();

        try
        {
            var slot = grid.SlotFromRowIndex(0);
            Assert.True(grid.UpdateSelectionAndCurrency(column.Index, slot, DataGridSelectionAction.SelectCurrent, scrollIntoView: false));
            grid.UpdateLayout();

            Assert.True(grid.BeginEdit());
            grid.UpdateLayout();

            var cell = FindCell(grid, item, column.Index);
            var row = FindRow(grid, item);
            var textBox = Assert.IsType<TextBox>(cell.Content);

            textBox.Text = string.Empty;
            Assert.False(grid.CommitEdit());
            grid.UpdateLayout();

            Assert.False(grid.IsValid);
            Assert.False(cell.IsValid);
            Assert.False(row.IsValid);
            Assert.True(((IPseudoClasses)cell.Classes).Contains(":invalid"));
            Assert.True(((IPseudoClasses)row.Classes).Contains(":invalid"));
            Assert.True(DataValidationErrors.GetHasErrors(textBox));

            textBox.Text = "Valid";
            Assert.True(grid.CommitEdit());
            grid.UpdateLayout();

            Assert.True(grid.IsValid);
            Assert.True(cell.IsValid);
            Assert.True(row.IsValid);
            Assert.False(((IPseudoClasses)cell.Classes).Contains(":invalid"));
            Assert.False(((IPseudoClasses)row.Classes).Contains(":invalid"));
            Assert.False(DataValidationErrors.GetHasErrors(textBox));
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void DataValidationErrors_theme_uses_grid_corner_radius()
    {
        var (grid, root, item, column) = CreateTextValidationGrid(DataGridTheme.Fluent);

        try
        {
            var slot = grid.SlotFromRowIndex(0);
            Assert.True(grid.UpdateSelectionAndCurrency(column.Index, slot, DataGridSelectionAction.SelectCurrent, scrollIntoView: false));
            grid.UpdateLayout();

            Assert.True(grid.BeginEdit());
            grid.UpdateLayout();

            var cell = FindCell(grid, item, column.Index);
            var textBox = Assert.IsType<TextBox>(cell.Content);
            var validation = textBox.GetVisualDescendants().OfType<DataValidationErrors>().First();

            Assert.True(grid.TryFindResource("DataGridCellDataValidationErrorsTheme", out var themeResource));
            Assert.Same(themeResource, validation.Theme);

            Assert.True(grid.TryFindResource("DataGridCellCornerRadius", out var cornerResource));
            Assert.Equal((CornerRadius)cornerResource!, validation.CornerRadius);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void Editable_columns_report_validation_errors()
    {
        var (grid, root, item) = CreateValidationGrid();

        try
        {
            var slot = grid.SlotFromRowIndex(0);

            foreach (var column in grid.ColumnsInternal)
            {
                if (!ColumnValidationCases.TryGetValue(column.Header?.ToString() ?? string.Empty, out var testCase))
                {
                    continue;
                }

                Assert.True(grid.UpdateSelectionAndCurrency(column.Index, slot, DataGridSelectionAction.SelectCurrent, scrollIntoView: false));
                grid.UpdateLayout();

                Assert.True(grid.BeginEdit());
                grid.UpdateLayout();

                var cell = FindCell(grid, item, column.Index);
                var editingElement = Assert.IsAssignableFrom<Control>(cell.Content);

                testCase.SetValue(editingElement, false);

                Assert.False(grid.CommitEdit());
                grid.UpdateLayout();

                Assert.False(cell.IsValid);
                Assert.True(DataValidationErrors.GetHasErrors(editingElement));

                testCase.SetValue(editingElement, true);

                Assert.True(grid.CommitEdit());
                grid.UpdateLayout();

                Assert.True(cell.IsValid);
                Assert.False(DataValidationErrors.GetHasErrors(editingElement));
            }
        }
        finally
        {
            root.Close();
        }
    }

    private static readonly IReadOnlyList<string> Categories = new[] { "Hardware", "Software", "Services" };
    private static readonly IReadOnlyList<string> Statuses = new[] { "Draft", "Active", "Closed" };

    private static readonly Dictionary<string, ColumnValidationCase> ColumnValidationCases = new()
    {
        ["Name"] = new ColumnValidationCase((control, valid) =>
        {
            var textBox = (TextBox)control;
            textBox.Text = valid ? "Gamma" : string.Empty;
        }),
        ["Category"] = new ColumnValidationCase((control, valid) =>
        {
            var autoComplete = (AutoCompleteBox)control;
            autoComplete.Text = valid ? Categories[0] : "Invalid";
        }),
        ["Status"] = new ColumnValidationCase((control, valid) =>
        {
            var comboBox = (ComboBox)control;
            comboBox.Text = valid ? Statuses[1] : "Bogus";
        }),
        ["Phone"] = new ColumnValidationCase((control, valid) =>
        {
            var masked = (MaskedTextBox)control;
            masked.Text = valid ? "(555) 010-1000" : string.Empty;
        }),
        ["Price"] = new ColumnValidationCase((control, valid) =>
        {
            var numeric = (NumericUpDown)control;
            numeric.Value = valid ? 25m : 12m;
        }),
        ["Due"] = new ColumnValidationCase((control, valid) =>
        {
            var picker = (CalendarDatePicker)control;
            picker.SelectedDate = valid ? NextWeekday(DateTime.Today.AddDays(1)) : DateTime.Today.AddDays(-1);
        }),
        ["Start"] = new ColumnValidationCase((control, valid) =>
        {
            var picker = (TimePicker)control;
            picker.SelectedTime = valid ? new TimeSpan(10, 0, 0) : new TimeSpan(7, 0, 0);
        }),
        ["Rating"] = new ColumnValidationCase((control, valid) =>
        {
            var slider = (Slider)control;
            slider.Value = valid ? 3.0 : 0.5;
        }),
        ["Active"] = new ColumnValidationCase((control, valid) =>
        {
            var toggle = (ToggleSwitch)control;
            toggle.IsChecked = valid;
        }),
        ["Pinned"] = new ColumnValidationCase((control, valid) =>
        {
            var toggle = (ToggleButton)control;
            toggle.IsChecked = valid;
        }),
        ["Approved"] = new ColumnValidationCase((control, valid) =>
        {
            var checkBox = (CheckBox)control;
            checkBox.IsChecked = valid;
        }),
        ["Website"] = new ColumnValidationCase((control, valid) =>
        {
            var textBox = (TextBox)control;
            textBox.Text = valid ? "https://example.com" : "not-a-url";
        })
    };

    private static (DataGrid grid, Window root, ValidationItem item, DataGridTextColumn column) CreateTextValidationGrid(DataGridTheme theme = DataGridTheme.Simple)
    {
        var item = new ValidationItem(Categories, Statuses)
        {
            Name = "Alpha",
            Category = Categories[0],
            Status = Statuses[0],
            Phone = "(555) 010-1000",
            Price = 25m,
            DueDate = NextWeekday(DateTime.Today.AddDays(1)),
            StartTime = new TimeSpan(10, 0, 0),
            Rating = 3.0,
            IsActive = true,
            IsPinned = true,
            IsApproved = true,
            Website = "https://example.com"
        };

        var items = new ObservableCollection<ValidationItem> { item };

        var root = new Window
        {
            Width = 800,
            Height = 400
        };

        root.SetThemeStyles(theme);

        var grid = new DataGrid
        {
            ItemsSource = items,
            AutoGenerateColumns = false
        };

        var nameColumn = new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(ValidationItem.Name))
        };

        grid.ColumnsInternal.Add(nameColumn);
        root.Content = grid;
        root.Show();
        grid.UpdateLayout();

        return (grid, root, item, nameColumn);
    }

    private static (DataGrid grid, Window root, ValidationItem item) CreateValidationGrid()
    {
        var item = new ValidationItem(Categories, Statuses)
        {
            Name = "Alpha",
            Category = Categories[0],
            Status = Statuses[1],
            Phone = "(555) 010-1000",
            Price = 25m,
            DueDate = NextWeekday(DateTime.Today.AddDays(1)),
            StartTime = new TimeSpan(10, 0, 0),
            Rating = 3.0,
            IsActive = true,
            IsPinned = true,
            IsApproved = true,
            Website = "https://example.com"
        };

        var items = new ObservableCollection<ValidationItem> { item };

        var root = new Window
        {
            Width = 1200,
            Height = 400
        };

        root.SetThemeStyles(DataGridTheme.Fluent);

        var grid = new DataGrid
        {
            ItemsSource = items,
            AutoGenerateColumns = false
        };

        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(ValidationItem.Name))
        });

        grid.ColumnsInternal.Add(new DataGridAutoCompleteColumn
        {
            Header = "Category",
            Binding = new Binding(nameof(ValidationItem.Category)),
            ItemsSource = Categories,
            FilterMode = AutoCompleteFilterMode.Contains,
            MinimumPrefixLength = 1
        });

        grid.ColumnsInternal.Add(new DataGridComboBoxColumn
        {
            Header = "Status",
            ItemsSource = Statuses,
            IsEditable = true,
            TextBinding = new Binding(nameof(ValidationItem.Status))
        });

        grid.ColumnsInternal.Add(new DataGridMaskedTextColumn
        {
            Header = "Phone",
            Binding = new Binding(nameof(ValidationItem.Phone)),
            Mask = "(000) 000-0000"
        });

        grid.ColumnsInternal.Add(new DataGridNumericColumn
        {
            Header = "Price",
            Binding = new Binding(nameof(ValidationItem.Price)),
            Minimum = 0,
            Maximum = 500,
            Increment = 1
        });

        grid.ColumnsInternal.Add(new DataGridDatePickerColumn
        {
            Header = "Due",
            Binding = new Binding(nameof(ValidationItem.DueDate)),
            SelectedDateFormat = CalendarDatePickerFormat.Short
        });

        grid.ColumnsInternal.Add(new DataGridTimePickerColumn
        {
            Header = "Start",
            Binding = new Binding(nameof(ValidationItem.StartTime)),
            ClockIdentifier = "24HourClock"
        });

        grid.ColumnsInternal.Add(new DataGridSliderColumn
        {
            Header = "Rating",
            Binding = new Binding(nameof(ValidationItem.Rating)),
            Minimum = 0,
            Maximum = 5,
            TickFrequency = 0.5,
            IsSnapToTickEnabled = true
        });

        grid.ColumnsInternal.Add(new DataGridToggleSwitchColumn
        {
            Header = "Active",
            Binding = new Binding(nameof(ValidationItem.IsActive))
        });

        grid.ColumnsInternal.Add(new DataGridToggleButtonColumn
        {
            Header = "Pinned",
            Binding = new Binding(nameof(ValidationItem.IsPinned))
        });

        grid.ColumnsInternal.Add(new DataGridCheckBoxColumn
        {
            Header = "Approved",
            Binding = new Binding(nameof(ValidationItem.IsApproved))
        });

        grid.ColumnsInternal.Add(new DataGridHyperlinkColumn
        {
            Header = "Website",
            Binding = new Binding(nameof(ValidationItem.Website)),
            ContentBinding = new Binding(nameof(ValidationItem.Website))
        });

        root.Content = grid;
        root.Show();
        grid.UpdateLayout();

        return (grid, root, item);
    }

    private static DataGridCell FindCell(DataGrid grid, ValidationItem item, int columnIndex)
    {
        return grid.GetVisualDescendants()
            .OfType<DataGridCell>()
            .First(cell => cell.OwningColumn?.Index == columnIndex && ReferenceEquals(cell.DataContext, item));
    }

    private static DataGridRow FindRow(DataGrid grid, ValidationItem item)
    {
        return grid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .First(row => ReferenceEquals(row.DataContext, item));
    }

    private static DateTime NextWeekday(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private sealed record ColumnValidationCase(Action<Control, bool> SetValue);

    private sealed class ValidationItem : INotifyPropertyChanged
    {
        private readonly HashSet<string> _categories;
        private readonly HashSet<string> _statuses;
        private string _name = string.Empty;
        private string? _category;
        private string? _status;
        private string? _phone;
        private decimal _price;
        private DateTime? _dueDate;
        private TimeSpan? _startTime;
        private double _rating;
        private bool _isActive;
        private bool _isPinned;
        private bool _isApproved;
        private string? _website;

        public ValidationItem(IEnumerable<string> categories, IEnumerable<string> statuses)
        {
            _categories = new HashSet<string>(categories);
            _statuses = new HashSet<string>(statuses);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                RequireText(value, "Name is required.");
                SetProperty(ref _name, value);
            }
        }

        public string? Category
        {
            get => _category;
            set
            {
                RequireChoice(value, _categories, "Category must match the suggestions list.");
                SetProperty(ref _category, value);
            }
        }

        public string? Status
        {
            get => _status;
            set
            {
                RequireChoice(value, _statuses, "Status must be one of the defined options.");
                SetProperty(ref _status, value);
            }
        }

        public string? Phone
        {
            get => _phone;
            set
            {
                RequireText(value, "Phone is required.");
                if (value!.Count(char.IsDigit) != 10)
                {
                    throw new DataValidationException("Phone must include 10 digits.");
                }

                SetProperty(ref _phone, value);
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 10m || value % 5m != 0)
                {
                    throw new DataValidationException("Price must be at least 10 and in increments of 5.");
                }

                SetProperty(ref _price, value);
            }
        }

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                RequireValue(value, "Due date is required.");
                var date = value!.Value.Date;
                if (date < DateTime.Today)
                {
                    throw new DataValidationException("Due date must be today or later.");
                }

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    throw new DataValidationException("Due date cannot fall on a weekend.");
                }

                SetProperty(ref _dueDate, value);
            }
        }

        public TimeSpan? StartTime
        {
            get => _startTime;
            set
            {
                RequireValue(value, "Start time is required.");
                var time = value!.Value;
                if (time < new TimeSpan(9, 0, 0) || time > new TimeSpan(17, 0, 0))
                {
                    throw new DataValidationException("Start time must be between 09:00 and 17:00.");
                }

                SetProperty(ref _startTime, value);
            }
        }

        public double Rating
        {
            get => _rating;
            set
            {
                if (value < 1.0 || value > 4.5)
                {
                    throw new DataValidationException("Rating must be between 1.0 and 4.5.");
                }

                SetProperty(ref _rating, value);
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (!value)
                {
                    throw new DataValidationException("Active must stay enabled.");
                }

                SetProperty(ref _isActive, value);
            }
        }

        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                if (!value)
                {
                    throw new DataValidationException("Pinned must stay enabled.");
                }

                SetProperty(ref _isPinned, value);
            }
        }

        public bool IsApproved
        {
            get => _isApproved;
            set
            {
                if (!value)
                {
                    throw new DataValidationException("Approval is required.");
                }

                SetProperty(ref _isApproved, value);
            }
        }

        public string? Website
        {
            get => _website;
            set
            {
                RequireText(value, "Website is required.");
                if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new DataValidationException("Website must be a valid http/https URL.");
                }

                SetProperty(ref _website, value);
            }
        }

        private static void RequireValue<T>(T? value, string message) where T : struct
        {
            if (!value.HasValue)
            {
                throw new DataValidationException(message);
            }
        }

        private static void RequireText(string? value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DataValidationException(message);
            }
        }

        private static void RequireChoice(string? value, HashSet<string> allowed, string message)
        {
            if (string.IsNullOrWhiteSpace(value) || !allowed.Contains(value))
            {
                throw new DataValidationException(message);
            }
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
