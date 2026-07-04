using System.Text;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class QueryUtils
{
	internal static ArchetypeQuery DuplicateQuery(ArchetypeQuery query)
	{
		QueryFilter.FilterCondition condition = query.Filter.Condition;
		ComponentTypes types = condition.AllComponents;
		types.Add(in query.ComponentTypes);
		QueryFilter queryFilter = new QueryFilter();
		bool num = condition.WithoutAnyTags.Has<Disabled>();
		queryFilter.WithDisabled();
		Tags tags = condition.WithoutAnyTags;
		if (num)
		{
			tags.Add<Disabled>();
		}
		queryFilter.AllComponents(in types);
		queryFilter.AnyComponents(condition.AnyComponents);
		queryFilter.WithoutAllComponents(condition.WithoutAllComponents);
		queryFilter.WithoutAnyComponents(condition.WithoutAnyComponents);
		queryFilter.AllTags(condition.AllTags);
		queryFilter.AnyTags(condition.AnyTags);
		queryFilter.WithoutAllTags(condition.WithoutAllTags);
		queryFilter.WithoutAnyTags(in tags);
		return query.Store.Query(queryFilter);
	}

	internal static string QueryAsCode(ArchetypeQuery query)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Clear();
		stringBuilder.Append("store.Query");
		if (query.ComponentTypes.Count > 0)
		{
			stringBuilder.Append('<');
			AppendComponentTypes(stringBuilder, query.ComponentTypes);
			stringBuilder.Append('>');
		}
		stringBuilder.Append("()");
		QueryFilter.FilterCondition condition = query.Filter.Condition;
		AppendComponentFilter(stringBuilder, "AllComponents", condition.AllComponents);
		AppendComponentFilter(stringBuilder, "AnyComponents", condition.AnyComponents);
		AppendComponentFilter(stringBuilder, "WithoutAllComponents", condition.WithoutAllComponents);
		AppendComponentFilter(stringBuilder, "WithoutAnyComponents", condition.WithoutAnyComponents);
		AppendTagFilter(stringBuilder, "AllTags", condition.AllTags);
		AppendTagFilter(stringBuilder, "AnyTags", condition.AnyTags);
		AppendTagFilter(stringBuilder, "WithoutAllTags", condition.WithoutAllTags);
		AppendTagFilter(stringBuilder, "WithoutAnyTags", condition.WithoutAnyTags);
		stringBuilder.Append(';');
		return stringBuilder.ToString();
	}

	private static void AppendComponentFilter(StringBuilder sb, string name, ComponentTypes types)
	{
		if (types.Count != 0)
		{
			sb.AppendLine();
			sb.Append("    .");
			sb.Append(name);
			sb.Append("(ComponentTypes.Get<");
			AppendComponentTypes(sb, types);
			sb.Append(">())");
		}
	}

	private static void AppendTagFilter(StringBuilder sb, string name, Tags tags)
	{
		if (tags.Count != 0)
		{
			sb.AppendLine();
			sb.Append("    .");
			sb.Append(name);
			sb.Append("(Tags.Get<");
			AppendTags(sb, tags);
			sb.Append(">())");
		}
	}

	internal static void AppendComponentTypes(StringBuilder sb, ComponentTypes types)
	{
		bool flag = true;
		foreach (ComponentType item in types)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sb.Append(", ");
			}
			sb.Append(item.Name);
		}
	}

	internal static void AppendTags(StringBuilder sb, Tags tags)
	{
		bool flag = true;
		foreach (TagType item in tags)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sb.Append(", ");
			}
			sb.Append(item.Name);
		}
	}
}
