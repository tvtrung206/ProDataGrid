using System.Collections.ObjectModel;
using System.Linq;
using DataGridSample.Models;

namespace DataGridSample.ViewModels
{
    public class CellSelectionOnlyViewModel
    {
        public CellSelectionOnlyViewModel()
        {
            Items = new ObservableCollection<Country>(Countries.All.Take(18).ToList());
        }

        public ObservableCollection<Country> Items { get; }
    }
}
