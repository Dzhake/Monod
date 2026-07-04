using System;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class BoolDrawer : TypeDrawer
{
	public override int DefaultWidth => 100;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<bool>(out bool value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (ImGui.Checkbox("##field", ref value))
		{
			drawValue.SetValue(value);
		}
		return TypeDrawer.Flags();
	}
}
