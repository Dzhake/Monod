using System.Collections.Generic;
using ImGuiNET;

namespace Friflo.EcGui;

public static class ActiveItem<T>
{
	private static T? _activeValue;

	internal static bool IsActive(in DrawValue drawValue)
	{
		return ActiveWidget.IsActive(in drawValue);
	}

	public static bool SetValue(in DrawValue drawValue, T value)
	{
		if (!ActiveWidget.IsActive(in drawValue))
		{
			return false;
		}
		if (EqualityComparer<T>.Default.Equals(value, _activeValue))
		{
			return false;
		}
		drawValue.SetValue(value);
		_activeValue = value;
		return true;
	}

	public static bool SetActiveState(in DrawValue drawValue, T value)
	{
		if (ImGui.IsItemDeactivated() && ActiveWidget.IsActive(in drawValue))
		{
			ActiveWidget.identifier = default(InputIdentifier);
		}
		if (ImGui.IsItemActivated())
		{
			_activeValue = value;
			ActiveWidget.identifier.Set(in drawValue);
			return true;
		}
		return false;
	}
}
