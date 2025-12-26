using Avalonia.Automation.Peers;

namespace Avalonia.Controls.Automation.Peers;

#if !DATAGRID_INTERNAL
public
#else
internal
#endif
class DataGridColumnHeaderAutomationPeer : ContentControlAutomationPeer
{
    public DataGridColumnHeaderAutomationPeer(DataGridColumnHeader owner)
        : base(owner)
    {
    }

    public new DataGridColumnHeader Owner => (DataGridColumnHeader)base.Owner;

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.HeaderItem;
    }

    protected override bool IsContentElementCore() => false;

    protected override bool IsControlElementCore() => true;
}
