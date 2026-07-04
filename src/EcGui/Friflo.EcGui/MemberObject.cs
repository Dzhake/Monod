using System;
using System.Text;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class MemberObject : IMember
{
	private readonly MemberDrawer memberDrawer;

	private readonly MemberPath objectPath;

	private readonly IObjectDrawer objectDrawer;

	private readonly IMember[] members;

	private bool expandObject;

	public override string ToString()
	{
		return objectPath.ToString();
	}

	internal MemberObject(MemberDrawer memberDrawer, IObjectDrawer objectDrawer, IMember[] members)
	{
		this.memberDrawer = memberDrawer;
		objectPath = memberDrawer.member;
		this.objectDrawer = objectDrawer;
		this.members = members;
	}

	public void DrawMemberNode(DrawNode drawNode)
	{
		Exception exception = null;
		DrawValue drawValue = default(DrawValue);
		bool flag = false;
		if (objectDrawer is IClassDrawer classDrawer)
		{
			drawValue = new DrawValue(drawNode.context, in memberDrawer, DrawValueFlags.Value);
			flag = classDrawer.GetObject(in drawValue, out exception) == null || exception != null;
		}
		ImGui.SetNextItemOpen(expandObject);
		ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, enabled: true);
		if (flag)
		{
			drawNode.PushClipRect();
		}
		expandObject = ImGui.TreeNodeEx(memberDrawer.label, flag ? (ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanTextWidth | ImGuiTreeNodeFlags.NavLeftJumpsBackHere) : (ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.NavLeftJumpsBackHere));
		if (flag)
		{
			drawNode.PopClipRect();
		}
		drawNode.inspector.memberDepth++;
		ImGui.PopItemFlag();
		if (drawNode.ShowTooltips && ImGui.BeginItemTooltip())
		{
			StringBuilder sb = TextUtils.Clear().Append("Type: ");
			TypeUtils.AppendTypeName(sb, objectPath.memberType);
			ImGui.Text(TextUtils.AsSpan(sb));
			ImGui.EndTooltip();
		}
		if (flag)
		{
			ImGui.SameLine(drawNode.context.rect.left);
			ImGui.SetNextItemWidth(drawNode.Size.X);
			if (exception != null)
			{
				drawValue.DrawException(exception);
			}
			else
			{
				string input = "null";
				ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.frameBg);
				ImGui.InputText("##null", ref input, (uint)input.Length, ImGuiInputTextFlags.ReadOnly);
				ImGui.PopStyleColor();
			}
		}
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
		if (EntityInspector.MorePopup("generic_more", out var _))
		{
			bool enabled = IsReferenceType();
			if (ImGui.MenuItem("Create " + objectPath.memberType.Name, enabled))
			{
				try
				{
					CreateMemberInstance(drawNode.Entity);
				}
				catch (Exception exception2)
				{
					drawNode.context.onError?.Invoke("Create " + objectPath.memberType.Name + " - failed", exception2);
				}
			}
			if (ImGui.MenuItem("Remove instance", enabled))
			{
				try
				{
					SetMember(drawNode.Entity, null);
				}
				catch (Exception exception3)
				{
					drawNode.context.onError?.Invoke("Remove instance - failed", exception3);
				}
			}
			ImGui.Separator();
			if (ImGui.MenuItem("Add all columns"))
			{
				IMember[] array = members;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AddExplorerColumns(drawNode.explorer);
				}
			}
			ImGui.EndPopup();
		}
		if (expandObject)
		{
			for (int j = 0; j < members.Length; j++)
			{
				IMember member = members[j];
				if (!drawNode.inspector.filterActive || member.HasFilterMatches(drawNode.inspector.filter))
				{
					EcUtils.ID.PushID(j);
					member.DrawMemberNode(drawNode);
					EcUtils.ID.PopID();
				}
			}
			ImGui.TreePop();
		}
		drawNode.inspector.memberDepth--;
		ImGui.PopStyleVar();
	}

	private bool IsReferenceType()
	{
		return !objectPath.memberType.IsValueType;
	}

	private void CreateMemberInstance(Entity entity)
	{
		object instance = TypeUtils.CreateInstance(objectPath.memberType);
		SetMember(entity, instance);
	}

	private void SetMember(Entity entity, object? instance)
	{
		typeof(EntityUtils).GetMethod("SetEntityComponentMember").MakeGenericMethod(objectPath.memberType).Invoke(null, new object[5] { entity, objectPath, instance, null, null });
	}

	public void AddExplorerColumns(QueryExplorer explorer)
	{
	}

	public bool HasFilterMatches(string filter)
	{
		if (objectPath.path.Contains(filter, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		IMember[] array = members;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].HasFilterMatches(filter))
			{
				return true;
			}
		}
		return false;
	}
}
