using System.Numerics;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

public abstract class TypeDrawer
{
	public virtual int DefaultWidth => 200;

	public virtual string[] SortFields => new string[1] { "" };

	public virtual string[]? FormatFields => null;

	public abstract ItemFlags DrawValue(in DrawValue drawValue);

	public virtual void Format(MemberFormat format)
	{
		format.NotImplemented();
	}

	public static ItemFlags Flags()
	{
		ItemFlags itemFlags = ((!ImGui.IsItemFocused()) ? ItemFlags.Present : (ItemFlags.Present | ItemFlags.Focused));
		if (ImGui.BeginPopupContextItem("ctx"))
		{
			ImGui.EndPopup();
			itemFlags |= ItemFlags.ContextMenu;
		}
		return itemFlags;
	}

	internal static void PushItemStyle()
	{
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4f, 0f));
	}

	internal static void PopItemStyle()
	{
		ImGui.PopStyleVar();
	}
}
