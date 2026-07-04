using System.Numerics;
using Friflo.EcGui.Friflo.EcGui;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

public static class EcGui
{
	public static class Setup
	{
		public static void RegisterTypeDrawer<T>(TypeDrawer drawer, string? domain = null)
		{
			TypeDrawerUtils.RegisterTypeDrawer(typeof(T), drawer, domain);
		}

		public static void AddOnMemberChanged<T>(OnMemberChanged<T> handler)
		{
			MemberChangedHandlers.Add(handler);
		}

		public static void RemoveOnMemberChanged<T>(OnMemberChanged<T> handler)
		{
			MemberChangedHandlers.Remove(handler);
		}

		public static void SetDefaultStyles()
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			style.DockingSeparatorSize = 8f;
			style.WindowRounding = 6f;
			style.PopupRounding = 12f;
			style.WindowMenuButtonPosition = ImGuiDir.None;
			style.HoverStationaryDelay = 0.5f;
			style.ScrollbarSize = 20f;
			RangeAccessor<Vector4> colors = style.Colors;
			colors[38] = colors[2];
			colors[37] = colors[11];
			colors[34] = new Vector4(0f, 0f, 0f, 0f);
			colors[35] = colors[2];
		}
	}

	public static readonly QueryExplorer Explorer = new QueryExplorer();

	public static readonly EntityInspector Inspector = new EntityInspector(Explorer);

	public static void AddExplorerStore(string name, EntityStore store)
	{
		Explorer.AddStore(name, store);
	}

	public static void AddExplorerQuery(string name, ArchetypeQuery query)
	{
		Explorer.AddQuery(name, query);
	}

	public static void AddExplorerSystems(SystemRoot system)
	{
		Explorer.AddSystems(system);
	}

	public static void HistorySnapshot()
	{
		FieldHistories.AddHistories();
	}

	private static void SetExplorerWindow(ref Vector2? pos, ref Vector2? size, ImGuiCond cond)
	{
		ImGuiUtils.DrawImGuiWindows();
		GlobalColors.UpdateStyles(forceUpdate: false);
		Vector2 valueOrDefault = pos.GetValueOrDefault();
		if (!pos.HasValue)
		{
			valueOrDefault = new Vector2(UI.Scl(10f), UI.Scl(10f));
			pos = valueOrDefault;
		}
		valueOrDefault = size.GetValueOrDefault();
		if (!size.HasValue)
		{
			valueOrDefault = new Vector2(UI.Scl(1000f), UI.Scl(1000f));
			size = valueOrDefault;
		}
		ImGui.SetNextWindowPos(pos.Value, cond);
		ImGui.SetNextWindowSize(size.Value, cond);
		ImGui.SetNextWindowBgAlpha(1f);
	}

	private static void SetInspectorWindow(ref Vector2? pos, ref Vector2? size, ImGuiCond cond)
	{
		GlobalColors.UpdateStyles(forceUpdate: false);
		Vector2 valueOrDefault = pos.GetValueOrDefault();
		if (!pos.HasValue)
		{
			valueOrDefault = new Vector2(UI.Scl(1020f), UI.Scl(10f));
			pos = valueOrDefault;
		}
		valueOrDefault = size.GetValueOrDefault();
		if (!size.HasValue)
		{
			valueOrDefault = new Vector2(UI.Scl(800f), UI.Scl(700f));
			size = valueOrDefault;
		}
		ImGui.SetNextWindowPos(pos.Value, cond);
		ImGui.SetNextWindowSize(size.Value, cond);
		ImGui.SetNextWindowBgAlpha(1f);
	}

	public static void ExplorerWindow(Vector2? pos = null, Vector2? size = null, ImGuiCond cond = ImGuiCond.FirstUseEver)
	{
		SetExplorerWindow(ref pos, ref size, cond);
		if (ImGui.Begin("Explorer"))
		{
			Explorer.Draw();
		}
		ImGui.End();
	}

	public static void InspectorWindow(Vector2? pos = null, Vector2? size = null, ImGuiCond cond = ImGuiCond.FirstUseEver)
	{
		SetInspectorWindow(ref pos, ref size, cond);
		if (ImGui.Begin("Inspector"))
		{
			Inspector.Draw();
		}
		ImGui.End();
	}
}
