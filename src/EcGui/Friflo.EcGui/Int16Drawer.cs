using Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class Int16Drawer : TypeDrawer
{
	public override int DefaultWidth => 130;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<short>(out short value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		int value2 = value;
		if (EcUtils.InputInt32(ref value2, in drawValue, out var flags))
		{
			ActiveItem<short>.SetValue(in drawValue, (short)value2);
		}
		ActiveItem<short>.SetActiveState(in drawValue, (short)value2);
		return flags;
	}
}
