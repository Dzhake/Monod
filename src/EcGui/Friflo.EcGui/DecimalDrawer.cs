using System;

namespace Friflo.EcGui;

internal sealed class DecimalDrawer : TypeDrawer
{
	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<decimal>(out decimal value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.InputDecimal(ref value, in drawValue, out var flags))
		{
			ActiveItem<decimal>.SetValue(in drawValue, value);
		}
		ActiveItem<decimal>.SetActiveState(in drawValue, value);
		return flags;
	}
}
