using Hexa.NET.ImGui;
using System;

namespace Friflo.EcGui;

internal sealed class UInt64Drawer : TypeDrawer
{
    public override unsafe ItemFlags DrawValue(in DrawValue drawValue)
    {
        if (!drawValue.GetValue<ulong>(out ulong value, out Exception exception))
        {
            return drawValue.DrawException(exception);
        }
        ulong* p_data = &value;
        if (ImGui.InputScalar("##field", ImGuiDataType.U64, p_data))
        {
            ActiveItem<ulong>.SetValue(in drawValue, value);
        }
        ActiveItem<ulong>.SetActiveState(in drawValue, value);
        return TypeDrawer.Flags();
    }
}
