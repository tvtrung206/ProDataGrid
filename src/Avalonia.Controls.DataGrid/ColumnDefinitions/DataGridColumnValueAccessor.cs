// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections;
using System.Globalization;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IDataGridColumnValueAccessor
    {
        Type ItemType { get; }

        Type ValueType { get; }

        bool CanWrite { get; }

        object GetValue(object item);

        void SetValue(object item, object value);
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridColumnValueAccessor<TItem, TValue> : IDataGridColumnValueAccessor
    {
        private readonly Func<TItem, TValue> _getter;
        private readonly Action<TItem, TValue> _setter;

        public DataGridColumnValueAccessor(Func<TItem, TValue> getter, Action<TItem, TValue> setter = null)
        {
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter;
        }

        public Type ItemType => typeof(TItem);

        public Type ValueType => typeof(TValue);

        public bool CanWrite => _setter != null;

        public object GetValue(object item)
        {
            if (item is null)
            {
                return null;
            }

            if (item is TItem typed)
            {
                return _getter(typed);
            }

            return null;
        }

        public void SetValue(object item, object value)
        {
            if (_setter == null)
            {
                throw new InvalidOperationException("Setter is not available for this accessor.");
            }

            if (item is not TItem typed)
            {
                throw new InvalidOperationException($"Expected item of type '{typeof(TItem)}' but received '{item?.GetType()}'.");
            }

            _setter(typed, value is TValue typedValue ? typedValue : (TValue)value);
        }
    }

    internal sealed class DataGridColumnValueAccessorComparer : IComparer
    {
        private readonly IDataGridColumnValueAccessor _accessor;
        private readonly CultureInfo _culture;

        public DataGridColumnValueAccessorComparer(IDataGridColumnValueAccessor accessor, CultureInfo culture = null)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _culture = culture ?? CultureInfo.CurrentCulture;
        }

        internal IDataGridColumnValueAccessor Accessor => _accessor;

        public int Compare(object x, object y)
        {
            var left = _accessor.GetValue(x);
            var right = _accessor.GetValue(y);

            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            if (left is string leftString && right is string rightString)
            {
                return _culture.CompareInfo.Compare(leftString, rightString);
            }

            if (left is IComparable comparable)
            {
                return comparable.CompareTo(right);
            }

            return Comparer.Default.Compare(left, right);
        }
    }
}
