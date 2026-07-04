using System;
using System.Reflection;
using ImGuiNET;

namespace Friflo.EcGui;

internal static class ObjectDrawerUtils
{
	internal static TypeDrawer CreateClassDrawer(Type type)
	{
		return (TypeDrawer)typeof(ClassDrawer<>).MakeGenericType(type).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(null);
	}

	internal static ItemFlags DrawType(in DrawValue drawValue, Type type)
	{
		string input = type.Name;
		ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.lightFrameBg);
		ImGui.InputText("##field", ref input, 0u, ImGuiInputTextFlags.ReadOnly);
		ImGui.PopStyleColor();
		return TypeDrawer.Flags();
	}
}
