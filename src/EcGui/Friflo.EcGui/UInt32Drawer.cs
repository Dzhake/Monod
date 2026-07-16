using Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class UInt32Drawer : TypeDrawer
{
	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<uint>(out uint value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		int value2 = (int)value;
		if (EcUtils.InputInt32(ref value2, in drawValue, out var flags))
		{
			ActiveItem<uint>.SetValue(in drawValue, (uint)value2);
		}
		ActiveItem<uint>.SetActiveState(in drawValue, (uint)value2);
		return flags;
	}
}
