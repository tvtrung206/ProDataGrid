using System;
using System.Collections.ObjectModel;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class LargeVariableHeightViewModel : ObservableObject
    {
        private static readonly string[] Descriptions = CreateDescriptionCache();

        public LargeVariableHeightViewModel()
        {
            Items = new ObservableCollection<VariableHeightItem>();
            Populate(10_000, seed: 1234);
        }

        public ObservableCollection<VariableHeightItem> Items { get; }

        public int ItemCount => Items.Count;

        private void Populate(int count, int seed)
        {
            for (int i = 0; i < count; i++)
            {
                int lineCount = (i + seed) % 10 + 1; // Deterministic distribution 1-10

                Items.Add(new VariableHeightItem
                {
                    Id = i + 1,
                    Title = $"Row {i + 1}",
                    Description = Descriptions[lineCount],
                    LineCount = lineCount,
                    ExpectedHeight = 20 + (lineCount * 16)
                });
            }
        }

        private static string[] CreateDescriptionCache()
        {
            var cache = new string[11]; // Index by line count 1..10
            for (int lineCount = 1; lineCount <= 10; lineCount++)
            {
                var lines = new string[lineCount];
                for (int i = 0; i < lineCount; i++)
                {
                    lines[i] = $"Sample text line {i + 1} for variable height rows.";
                }
                cache[lineCount] = string.Join(Environment.NewLine, lines);
            }
            return cache;
        }
    }
}
