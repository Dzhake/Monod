using System.Numerics;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal static class ImGuiUtils
{
	internal static void DrawImGuiWindows()
	{
		if (QueryExplorer.showImGuiDemoWindow)
		{
			ImGui.ShowDemoWindow();
		}
		if (QueryExplorer.showStyleEditor)
		{
			ImGui.SetNextWindowPos(new Vector2(UI.Scl(1000f), UI.Scl(100f)), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowSize(new Vector2(UI.Scl(1000f), UI.Scl(1200f)), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowBgAlpha(1f);
			if (ImGui.Begin("Style Editor", ref QueryExplorer.showStyleEditor, ImGuiWindowFlags.None))
			{
				ImGui.ShowStyleEditor();
			}
			ImGui.End();
		}
	}
}
