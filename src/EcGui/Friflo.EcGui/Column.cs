using System;
using Friflo.Engine.ECS;
using ImGuiNET;

namespace Friflo.EcGui;

internal abstract class Column
{
	internal string filterText = "";

	internal bool filterOk = true;

	private FieldFilter[] fieldFilters = Array.Empty<FieldFilter>();

	private TermOperator termOperator;

	private readonly Func<Entity, bool> andPredicate;

	private readonly Func<Entity, bool> orPredicate;

	private Func<Entity, bool>? predicate;

	internal bool HasFilter => fieldFilters.Length != 0;

	internal abstract string Name { get; }

	internal virtual string? Tooltip => null;

	internal abstract SchemaType? SchemaType { get; }

	internal abstract MemberPath? MemberPath { get; }

	internal abstract bool Sortable { get; }

	internal abstract int GetDefaultWidth(bool expanded);

	internal abstract ItemFlags DrawCell(DrawCell drawCell);

	internal virtual void Insert(Entity entity)
	{
	}

	internal virtual void Remove(Entity entity)
	{
	}

	internal abstract void ContextMenu(ContextMenu menu);

	internal Column()
	{
		andPredicate = AndFieldFilters;
		orPredicate = OrFieldFilters;
	}

	internal Func<Entity, bool> GetFilterPredicate()
	{
		if (fieldFilters.Length == 1)
		{
			return predicate;
		}
		if (termOperator != TermOperator.And)
		{
			return orPredicate;
		}
		return andPredicate;
	}

	internal void SetFilters(FieldFilter[] filters, TermOperator termOperator)
	{
		fieldFilters = filters;
		this.termOperator = termOperator;
		if (filters.Length == 1)
		{
			predicate = filters[0].FilterField;
		}
	}

	private bool AndFieldFilters(Entity entity)
	{
		FieldFilter[] array = fieldFilters;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].FilterField(entity))
			{
				return false;
			}
		}
		return true;
	}

	private bool OrFieldFilters(Entity entity)
	{
		FieldFilter[] array = fieldFilters;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].FilterField(entity))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetSortField(MemberPath[] sortableFields, MemberPath sortBy, out MemberPath result)
	{
		result = null;
		bool result2 = false;
		if (ImGui.BeginMenu("Sort by"))
		{
			foreach (MemberPath memberPath in sortableFields)
			{
				bool v = memberPath.path == sortBy.path;
				if (ImGui.Checkbox(memberPath.path, ref v))
				{
					result = memberPath;
					result2 = true;
				}
			}
			ImGui.EndMenu();
		}
		return result2;
	}
}
