using Friflo.EcGui.Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class Float32Drawer : TypeDrawer
{
	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<float>(out float value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.InputFloat(ref value, in drawValue, out var flags))
		{
			ActiveItem<float>.SetValue(in drawValue, value);
		}
		ActiveItem<float>.SetActiveState(in drawValue, value);
		return flags;
	}
}
