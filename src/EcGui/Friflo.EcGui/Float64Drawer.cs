using Friflo.EcGui.Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class Float64Drawer : TypeDrawer
{
	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<double>(out double value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.InputDouble(ref value, in drawValue, out var flags))
		{
			ActiveItem<double>.SetValue(in drawValue, value);
		}
		ActiveItem<double>.SetActiveState(in drawValue, value);
		return flags;
	}
}
