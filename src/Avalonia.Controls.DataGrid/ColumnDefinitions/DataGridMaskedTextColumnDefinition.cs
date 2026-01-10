// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System.Globalization;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridMaskedTextColumnDefinition : DataGridBoundColumnDefinition
    {
        private string _mask;
        private char? _promptChar;
        private bool? _asciiOnly;
        private bool? _hidePromptOnLeave;
        private bool? _resetOnPrompt;
        private bool? _resetOnSpace;
        private CultureInfo _culture;
        private string _watermark;

        public string Mask
        {
            get => _mask;
            set => SetProperty(ref _mask, value);
        }

        public char? PromptChar
        {
            get => _promptChar;
            set => SetProperty(ref _promptChar, value);
        }

        public bool? AsciiOnly
        {
            get => _asciiOnly;
            set => SetProperty(ref _asciiOnly, value);
        }

        public bool? HidePromptOnLeave
        {
            get => _hidePromptOnLeave;
            set => SetProperty(ref _hidePromptOnLeave, value);
        }

        public bool? ResetOnPrompt
        {
            get => _resetOnPrompt;
            set => SetProperty(ref _resetOnPrompt, value);
        }

        public bool? ResetOnSpace
        {
            get => _resetOnSpace;
            set => SetProperty(ref _resetOnSpace, value);
        }

        public CultureInfo Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }

        protected override DataGridColumn CreateColumnCore()
        {
            return new DataGridMaskedTextColumn();
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            base.ApplyColumnProperties(column, context);

            if (column is DataGridMaskedTextColumn maskedColumn)
            {
                maskedColumn.Mask = Mask;
                maskedColumn.Culture = Culture;
                maskedColumn.Watermark = Watermark;

                if (PromptChar.HasValue)
                {
                    maskedColumn.PromptChar = PromptChar.Value;
                }

                if (AsciiOnly.HasValue)
                {
                    maskedColumn.AsciiOnly = AsciiOnly.Value;
                }

                if (HidePromptOnLeave.HasValue)
                {
                    maskedColumn.HidePromptOnLeave = HidePromptOnLeave.Value;
                }

                if (ResetOnPrompt.HasValue)
                {
                    maskedColumn.ResetOnPrompt = ResetOnPrompt.Value;
                }

                if (ResetOnSpace.HasValue)
                {
                    maskedColumn.ResetOnSpace = ResetOnSpace.Value;
                }
            }
        }
    }
}
