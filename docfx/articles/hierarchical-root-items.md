# Observable Root Collections

`HierarchicalModel.SetRoots` accepts a collection of root items. When that collection implements `INotifyCollectionChanged` (for example, `ObservableCollection<T>`), the model listens for root additions/removals and updates the flattened view automatically. This is useful when your app starts with an empty root list and populates it later.

```csharp
var roots = new ObservableCollection<TreeNode>();

var model = new HierarchicalModel<TreeNode>(new HierarchicalOptions<TreeNode>
{
    ChildrenSelector = node => node.Children
});

model.SetRoots(roots);

// Later on...
roots.Add(new TreeNode("Root A"));
roots.Add(new TreeNode("Root B"));
```

Bind the model to a `DataGrid` by setting `HierarchicalModel` and enabling hierarchical rows. The grid will refresh as root items change.

See the `Hierarchical Root Items` page in the sample app (`src/DataGridSample`) for a live example.
