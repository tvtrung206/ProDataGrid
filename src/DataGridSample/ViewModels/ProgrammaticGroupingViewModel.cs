// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Collections;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ProgrammaticGroupingViewModel : ObservableObject
    {
        private readonly ObservableCollection<DocumentProperty> _items;
        private readonly RelayCommand _categoryThenGroupCommand;
        private readonly RelayCommand _insertGroupCommand;
        private readonly RelayCommand _replaceTopLevelCommand;
        private readonly RelayCommand _localeCategoryCommand;
        private readonly RelayCommand _clearGroupingCommand;
        private readonly RelayCommand _addPropertyCommand;
        private readonly RelayCommand _singleGroupCommand;
        private int _nextPropertyId = 1;

        public ProgrammaticGroupingViewModel()
        {
            _items = new ObservableCollection<DocumentProperty>(CreateSample());
            View = new DataGridCollectionView(_items);

            _categoryThenGroupCommand = new RelayCommand(_ => ApplyCategoryThenGroup());
            _insertGroupCommand = new RelayCommand(_ => ApplyInsertAtIndex());
            _replaceTopLevelCommand = new RelayCommand(_ => ApplyGroupThenCategory());
            _localeCategoryCommand = new RelayCommand(_ => ApplyLocaleThenCategory());
            _clearGroupingCommand = new RelayCommand(_ => ClearGrouping());
            _addPropertyCommand = new RelayCommand(_ => AddRandomProperty());
            _singleGroupCommand = new RelayCommand(_ => ApplyCategoryOnly());

            ApplyCategoryThenGroup();
        }

        public DataGridCollectionView View { get; }

        public ICommand CategoryThenGroupCommand => _categoryThenGroupCommand;

        public ICommand InsertGroupCommand => _insertGroupCommand;

        public ICommand ReplaceTopLevelCommand => _replaceTopLevelCommand;

        public ICommand LocaleCategoryCommand => _localeCategoryCommand;

        public ICommand ClearGroupingCommand => _clearGroupingCommand;

        public ICommand AddPropertyCommand => _addPropertyCommand;

        public ICommand SingleGroupCommand => _singleGroupCommand;

        private void ApplyCategoryThenGroup()
        {
            SetGroupDescriptions(groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Category)));
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Group)));
            });
        }

        private void ApplyInsertAtIndex()
        {
            SetGroupDescriptions(groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Category)));
                groups.Insert(1, new DataGridPathGroupDescription(nameof(DocumentProperty.Group)));
            });
        }

        private void ApplyGroupThenCategory()
        {
            SetGroupDescriptions(groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Group)));
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Category)));
            });
        }

        private void ApplyCategoryOnly()
        {
            SetGroupDescriptions(groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Category)));
            });
        }

        private void ApplyLocaleThenCategory()
        {
            SetGroupDescriptions(groups =>
            {
                var locale = new DataGridPathGroupDescription(nameof(DocumentProperty.Locale));
                // Encourage predictable ordering to match how a user might curate groups.
                locale.GroupKeys.Add("es-ES");
                locale.GroupKeys.Add("en-US");
                locale.GroupKeys.Add("(none)");

                groups.Add(locale);
                groups.Add(new DataGridPathGroupDescription(nameof(DocumentProperty.Category)));
            });
        }

        private void ClearGrouping()
        {
            View.GroupDescriptions.Clear();
            View.Refresh();
        }

        private void AddRandomProperty()
        {
            var suffix = _nextPropertyId++;
            var random = new Random(17 + suffix);
            var category = _items[random.Next(_items.Count)].Category;
            var group = _items[random.Next(_items.Count)].Group;
            var locale = random.NextDouble() > 0.5 ? "es-ES" : "en-US";
            _items.Insert(0, new DocumentProperty(category, group, $"Generated {suffix}", $"Value {suffix}", locale));
        }

        private void SetGroupDescriptions(Action<AvaloniaList<DataGridGroupDescription>> configure)
        {
            var groups = View.GroupDescriptions;
            groups.Clear();
            configure(groups);
            View.Refresh();
        }

        private static ObservableCollection<DocumentProperty> CreateSample()
        {
            return new ObservableCollection<DocumentProperty>(new[]
            {
                new DocumentProperty("Prices", "Identification", "Base price", "$22.00", "es-ES"),
                new DocumentProperty("Prices", "Identification", "Wholesale", "$17.20", "en-US"),
                new DocumentProperty("Prices", "Encoding", "Currency", "EUR", "es-ES"),
                new DocumentProperty("Prices", "Encoding", "Currency", "USD", "en-US"),
                new DocumentProperty("Metadata", "Identification", "Name", "Documento 1", "es-ES"),
                new DocumentProperty("Metadata", "Identification", "Name", "Document 1", "en-US"),
                new DocumentProperty("Metadata", "Encoding", "Language", "Spanish", "es-ES"),
                new DocumentProperty("Metadata", "Encoding", "Language", "English", "en-US"),
            });
        }
    }
}
