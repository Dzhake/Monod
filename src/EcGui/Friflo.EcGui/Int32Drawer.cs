using System;

namespace Friflo.EcGui;

internal sealed class Int32Drawer : TypeDrawer
{
	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<int>(out int value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.InputInt32(ref value, in drawValue, out var flags))
		{
			ActiveItem<int>.SetValue(in drawValue, value);
		}
		ActiveItem<int>.SetActiveState(in drawValue, value);
		return flags;
	}
}
