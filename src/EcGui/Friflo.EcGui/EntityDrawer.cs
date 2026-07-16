using System;
using Friflo.EcGui;
using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class EntityDrawer : TypeDrawer
{
	private static string _activeText = "";

	public override string[] SortFields => new string[1] { "Id" };

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		bool flag = false;
		string input;
		Entity value;
		if (ActiveItem<Entity>.IsActive(in drawValue))
		{
			input = _activeText;
			if (Parse(in drawValue, input, out value))
			{
				flag = IsValid(value);
			}
		}
		else
		{
			if (!drawValue.GetValue<Entity>(out value, out Exception exception))
			{
				return drawValue.DrawException(exception);
			}
			input = ((value.Id == 0) ? "null" : value.Id.ToString());
			flag = IsValid(value);
		}
		if (!flag)
		{
			ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.errorText);
		}
		if (ImGui.InputText("##field", ref input, 10u))
		{
			_activeText = input;
			if (Parse(in drawValue, input, out var entity))
			{
				ActiveItem<Entity>.SetValue(in drawValue, entity);
			}
		}
		if (!flag)
		{
			ImGui.PopStyleColor();
		}
		if (ActiveItem<Entity>.SetActiveState(in drawValue, value))
		{
			_activeText = input;
		}
		return TypeDrawer.Flags();
	}

	private static bool IsValid(Entity entity)
	{
		if (entity.Id == 0)
		{
			return true;
		}
		return !entity.IsNull;
	}

	private static bool Parse(in DrawValue drawValue, string text, out Entity entity)
	{
		entity = default(Entity);
		if (text == "null" || text == "0")
		{
			return true;
		}
		if (!int.TryParse(text, out var result))
		{
			return false;
		}
		if (drawValue.context.entity.Store.TryGetEntityById(result, out entity))
		{
			return true;
		}
		return false;
	}
}
