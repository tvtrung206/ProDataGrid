// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridButtonColumnDefinition : DataGridColumnDefinition
    {
        private object _content;
        private string _contentTemplateKey;
        private ICommand _command;
        private object _commandParameter;
        private ClickMode? _clickMode;
        private KeyGesture _hotKey;

        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string ContentTemplateKey
        {
            get => _contentTemplateKey;
            set => SetProperty(ref _contentTemplateKey, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public object CommandParameter
        {
            get => _commandParameter;
            set => SetProperty(ref _commandParameter, value);
        }

        public ClickMode? ClickMode
        {
            get => _clickMode;
            set => SetProperty(ref _clickMode, value);
        }

        public KeyGesture HotKey
        {
            get => _hotKey;
            set => SetProperty(ref _hotKey, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridButtonColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            if (column is DataGridButtonColumn buttonColumn)
            {
                buttonColumn.Content = Content;
                buttonColumn.ContentTemplate = ContentTemplateKey != null
                    ? context?.ResolveResource<IDataTemplate>(ContentTemplateKey)
                    : null;
                buttonColumn.Command = Command;
                buttonColumn.CommandParameter = CommandParameter;

                if (ClickMode.HasValue)
                {
                    buttonColumn.ClickMode = ClickMode.Value;
                }

                buttonColumn.HotKey = HotKey;
            }
        }
    }
}
