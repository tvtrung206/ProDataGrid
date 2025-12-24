using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Data;
using Avalonia.Media;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ValidationColumnsViewModel : ObservableObject
    {
        private static readonly string[] DefaultCategories =
        {
            "Hardware",
            "Software",
            "Services",
            "Support",
            "Other"
        };

        private static readonly string[] DefaultStatuses =
        {
            "Draft",
            "Active",
            "On Hold",
            "Closed"
        };

        public ValidationColumnsViewModel()
        {
            Categories = new ReadOnlyCollection<string>(DefaultCategories);
            Statuses = new ReadOnlyCollection<string>(DefaultStatuses);

            Items = new ObservableCollection<ValidationSampleItem>
            {
                new ValidationSampleItem(Categories, Statuses)
                {
                    Name = "Alpha",
                    Category = "Hardware",
                    Status = "Active",
                    Phone = "(555) 010-1001",
                    Price = 25m,
                    DueDate = NextWeekday(DateTime.Today.AddDays(2)),
                    StartTime = new TimeSpan(9, 0, 0),
                    Rating = 3.5,
                    IsActive = true,
                    IsPinned = true,
                    IsApproved = true,
                    Website = "https://example.com",
                    Progress = 65
                },
                new ValidationSampleItem(Categories, Statuses)
                {
                    Name = "Beta",
                    Category = "Software",
                    Status = "Draft",
                    Phone = "(555) 010-1002",
                    Price = 30m,
                    DueDate = NextWeekday(DateTime.Today.AddDays(5)),
                    StartTime = new TimeSpan(10, 0, 0),
                    Rating = 2.5,
                    IsActive = true,
                    IsPinned = true,
                    IsApproved = true,
                    Website = "https://avaloniaui.net",
                    Progress = 40
                }
            };

            FixRowCommand = new RelayCommand(FixRow, CanFixRow);
        }

        public ObservableCollection<ValidationSampleItem> Items { get; }

        public IReadOnlyList<string> Categories { get; }

        public IReadOnlyList<string> Statuses { get; }

        public RelayCommand FixRowCommand { get; }

        private static bool CanFixRow(object? parameter)
        {
            return parameter is ValidationSampleItem;
        }

        private void FixRow(object? parameter)
        {
            if (parameter is ValidationSampleItem item)
            {
                item.ResetToDefaults();
            }
        }

        private static DateTime NextWeekday(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            return date;
        }
    }

    public class ValidationSampleItem : ObservableObject
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
        private double _progress;
        private IImage? _thumbnail;

        public ValidationSampleItem(IReadOnlyCollection<string> categories, IReadOnlyCollection<string> statuses)
        {
            _categories = new HashSet<string>(categories);
            _statuses = new HashSet<string>(statuses);
        }

        public void ResetToDefaults()
        {
            Name = "Item";
            Category = _categories.FirstOrDefault() ?? "Hardware";
            Status = _statuses.FirstOrDefault() ?? "Draft";
            Phone = "(555) 010-0000";
            Price = 25m;
            DueDate = NextWeekday(DateTime.Today.AddDays(1));
            StartTime = new TimeSpan(9, 0, 0);
            Rating = 3.0;
            IsActive = true;
            IsPinned = true;
            IsApproved = true;
            Website = "https://example.com";
            Progress = 50;
            Thumbnail = null;
        }

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

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public IImage? Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
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

        private static DateTime NextWeekday(DateTime date)
        {
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            return date;
        }
    }
}
