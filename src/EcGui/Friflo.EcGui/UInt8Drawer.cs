using Friflo.EcGui.Friflo.EcGui;
using System;

namespace Friflo.EcGui;

internal sealed class UInt8Drawer : TypeDrawer
{
	public override int DefaultWidth => 100;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<byte>(out byte value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		int value2 = value;
		if (EcUtils.InputInt32(ref value2, in drawValue, out var flags))
		{
			ActiveItem<byte>.SetValue(in drawValue, (byte)value2);
		}
		ActiveItem<byte>.SetActiveState(in drawValue, (byte)value2);
		return flags;
	}
}
