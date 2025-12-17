// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace DataGridSample.Models
{
    public sealed class DocumentProperty
    {
        public DocumentProperty(string category, string group, string name, string value, string? locale = null)
        {
            Category = category;
            Group = group;
            Name = name;
            Value = value;
            Locale = locale;
        }

        public string Category { get; }

        public string Group { get; }

        public string Name { get; }

        public string Value { get; }

        public string? Locale { get; }
    }
}
