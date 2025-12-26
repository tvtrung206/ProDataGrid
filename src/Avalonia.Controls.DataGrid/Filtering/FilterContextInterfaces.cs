// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Avalonia.Controls.DataGridFiltering;

/// <summary>
/// Minimal contract for text-based filter contexts consumed by the shared filter templates.
/// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
interface IFilterTextContext
{
    string Label { get; }
    string? Text { get; set; }
    ICommand ApplyCommand { get; }
    ICommand ClearCommand { get; }
}

/// <summary>
/// Minimal contract for numeric range filter contexts.
/// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
interface IFilterNumberContext
{
    string Label { get; }
    double Minimum { get; }
    double Maximum { get; }
    double? MinValue { get; set; }
    double? MaxValue { get; set; }
    ICommand ApplyCommand { get; }
    ICommand ClearCommand { get; }
}

/// <summary>
/// Minimal contract for date range filter contexts.
/// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
interface IFilterDateContext
{
    string Label { get; }
    System.DateTimeOffset? From { get; set; }
    System.DateTimeOffset? To { get; set; }
    ICommand ApplyCommand { get; }
    ICommand ClearCommand { get; }
}

/// <summary>
/// Minimal contract for enum/multi-select filter contexts.
/// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
interface IFilterEnumContext
{
    string Label { get; }
    ObservableCollection<IEnumOption> Options { get; }
    ICommand ApplyCommand { get; }
    ICommand ClearCommand { get; }
}

/// <summary>
/// Option contract for enum/multi-select filter items.
/// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
interface IEnumOption
{
    string Display { get; }
    bool IsSelected { get; set; }
}
