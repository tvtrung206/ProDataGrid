// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridHierarchical;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class HierarchicalRootItemsViewModel : ObservableObject
    {
        public class TreeItem
        {
            public TreeItem(int id, string name)
            {
                Id = id;
                Name = name;
                Children = new ObservableCollection<TreeItem>();
            }

            public int Id { get; }

            public string Name { get; }

            public ObservableCollection<TreeItem> Children { get; }
        }

        private int _nextId = 1;
        private int _rootCount;
        private int _visibleCount;

        public HierarchicalRootItemsViewModel()
        {
            var options = new HierarchicalOptions<TreeItem>
            {
                ChildrenSelector = item => item.Children
            };

            Model = new HierarchicalModel<TreeItem>(options);
            Model.SetRoots(RootItems);

            AddRootCommand = new RelayCommand(_ => AddRoot());
            AddChildToLastRootCommand = new RelayCommand(_ => AddChildToLastRoot(), _ => RootItems.Count > 0);
            RemoveLastRootCommand = new RelayCommand(_ => RemoveLastRoot(), _ => RootItems.Count > 0);
            ClearRootsCommand = new RelayCommand(_ => ClearRoots(), _ => RootItems.Count > 0);
            SeedSampleCommand = new RelayCommand(_ => SeedSample());

            RootItems.CollectionChanged += (_, _) => UpdateCounts();
            Model.FlattenedChanged += (_, _) => UpdateCounts();

            UpdateCounts();
        }

        public ObservableCollection<TreeItem> RootItems { get; } = new();

        public HierarchicalModel<TreeItem> Model { get; }

        public int RootCount
        {
            get => _rootCount;
            private set => SetProperty(ref _rootCount, value);
        }

        public int VisibleCount
        {
            get => _visibleCount;
            private set => SetProperty(ref _visibleCount, value);
        }

        public RelayCommand AddRootCommand { get; }

        public RelayCommand AddChildToLastRootCommand { get; }

        public RelayCommand RemoveLastRootCommand { get; }

        public RelayCommand ClearRootsCommand { get; }

        public RelayCommand SeedSampleCommand { get; }

        private void AddRoot()
        {
            RootItems.Add(CreateRoot($"Root {_nextId}"));
        }

        private void AddChildToLastRoot()
        {
            if (RootItems.Count == 0)
            {
                return;
            }

            var root = RootItems[RootItems.Count - 1];
            root.Children.Add(CreateChild(root.Name));
        }

        private void RemoveLastRoot()
        {
            if (RootItems.Count == 0)
            {
                return;
            }

            RootItems.RemoveAt(RootItems.Count - 1);
        }

        private void ClearRoots()
        {
            RootItems.Clear();
        }

        private void SeedSample()
        {
            RootItems.Clear();
            RootItems.Add(CreateRoot("Alpha"));
            RootItems.Add(CreateRoot("Beta"));
            RootItems.Add(CreateRoot("Gamma"));
        }

        private TreeItem CreateRoot(string name)
        {
            var root = new TreeItem(_nextId++, name);
            root.Children.Add(new TreeItem(_nextId++, $"{name} - Child A"));
            root.Children.Add(new TreeItem(_nextId++, $"{name} - Child B"));
            root.Children[1].Children.Add(new TreeItem(_nextId++, $"{name} - Child B1"));
            return root;
        }

        private TreeItem CreateChild(string parentName)
        {
            var id = _nextId++;
            return new TreeItem(id, $"{parentName} - Child {id}");
        }

        private void UpdateCounts()
        {
            RootCount = RootItems.Count;
            VisibleCount = Model.Count;
            AddChildToLastRootCommand.RaiseCanExecuteChanged();
            RemoveLastRootCommand.RaiseCanExecuteChanged();
            ClearRootsCommand.RaiseCanExecuteChanged();
        }
    }
}
