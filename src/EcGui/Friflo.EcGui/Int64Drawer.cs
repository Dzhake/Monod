using Hexa.NET.ImGui;
using System;

namespace Friflo.EcGui;

internal sealed class Int64Drawer : TypeDrawer
{
    public override unsafe ItemFlags DrawValue(in DrawValue drawValue)
    {
        if (!drawValue.GetValue<long>(out long value, out Exception exception))
        {
            return drawValue.DrawException(exception);
        }
        long* p_data = &value;
        if (ImGui.InputScalar("##field", ImGuiDataType.S64, p_data))
        {
            ActiveItem<long>.SetValue(in drawValue, value);
        }
        ActiveItem<long>.SetActiveState(in drawValue, value);
        return TypeDrawer.Flags();
    }
}
