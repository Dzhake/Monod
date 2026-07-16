using Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class Int8Drawer : TypeDrawer
{
	public override int DefaultWidth => 100;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<sbyte>(out sbyte value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		int value2 = value;
		if (EcUtils.InputInt32(ref value2, in drawValue, out var flags))
		{
			ActiveItem<sbyte>.SetValue(in drawValue, (sbyte)value2);
		}
		ActiveItem<sbyte>.SetActiveState(in drawValue, (sbyte)value2);
		return flags;
	}
}
