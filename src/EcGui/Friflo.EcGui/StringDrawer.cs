using System;
using Friflo.EcGui.Friflo.EcGui;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class StringDrawer : TypeDrawer
{
	public override int DefaultWidth => 220;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<string>(out string value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (value == null)
		{
			value = string.Empty;
		}
		if (ImGui.InputText("##field", ref value, 100u))
		{
			ActiveItem<string>.SetValue(in drawValue, value);
		}
		ActiveItem<string>.SetActiveState(in drawValue, value);
		return TypeDrawer.Flags();
	}
}
