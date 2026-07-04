using Friflo.EcGui.Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class UInt16Drawer : TypeDrawer
{
	public override int DefaultWidth => 130;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<ushort>(out ushort value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		int value2 = value;
		if (EcUtils.InputInt32(ref value2, in drawValue, out var flags))
		{
			ActiveItem<ushort>.SetValue(in drawValue, (ushort)value2);
		}
		ActiveItem<ushort>.SetActiveState(in drawValue, (ushort)value2);
		return flags;
	}
}
