// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

using Avalonia.Interactivity;
using System;

namespace Avalonia.Controls
{
    /// <summary>
    /// Selection origin scoping helpers.
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    partial class DataGrid
    {
        private DataGridSelectionChangeSource _pendingSelectionChangeSource = DataGridSelectionChangeSource.Unknown;
        private RoutedEventArgs _pendingSelectionTriggerEvent;

        /// <summary>
        /// Begins a selection change scope that captures the origin information until disposed.
        /// </summary>
        internal IDisposable BeginSelectionChangeScope(DataGridSelectionChangeSource source, RoutedEventArgs triggerEvent = null, bool sticky = false)
        {
            var previousSource = _pendingSelectionChangeSource;
            var previousTrigger = _pendingSelectionTriggerEvent;

            _pendingSelectionChangeSource |= source;

            if (triggerEvent != null && _pendingSelectionTriggerEvent == null)
            {
                _pendingSelectionTriggerEvent = triggerEvent;
            }

            return new SelectionChangeScope(this, previousSource, previousTrigger, sticky);
        }

        internal DataGridSelectionChangeSource CurrentSelectionChangeSource => _pendingSelectionChangeSource;

        internal RoutedEventArgs CurrentSelectionTriggerEvent => _pendingSelectionTriggerEvent;

        private void RestoreSelectionChangeScope(DataGridSelectionChangeSource source, RoutedEventArgs triggerEvent)
        {
            _pendingSelectionChangeSource = source;
            _pendingSelectionTriggerEvent = triggerEvent;
        }

        private sealed class SelectionChangeScope : IDisposable
        {
            private DataGrid _owner;
            private readonly DataGridSelectionChangeSource _source;
            private readonly RoutedEventArgs _triggerEvent;
            private readonly bool _sticky;

            public SelectionChangeScope(DataGrid owner, DataGridSelectionChangeSource source, RoutedEventArgs triggerEvent, bool sticky)
            {
                _owner = owner;
                _source = source;
                _triggerEvent = triggerEvent;
                _sticky = sticky;
            }

            public void Dispose()
            {
                if (_owner != null && !_sticky)
                {
                    _owner.RestoreSelectionChangeScope(_source, _triggerEvent);
                    _owner = null;
                }
            }
        }
    }
}
