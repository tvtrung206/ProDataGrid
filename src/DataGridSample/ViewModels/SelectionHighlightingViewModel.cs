using System.Collections.ObjectModel;
using System.Linq;
using DataGridSample.Models;

namespace DataGridSample.ViewModels
{
    public class SelectionHighlightingViewModel
    {
        public SelectionHighlightingViewModel()
        {
            Items = new ObservableCollection<Country>(Countries.All.Take(16).ToList());
        }

        public ObservableCollection<Country> Items { get; }
    }
}
