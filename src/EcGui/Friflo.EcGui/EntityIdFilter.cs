using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class EntityIdFilter : FieldFilter
{
	private readonly MatchType matchType;

	private readonly int id;

	private readonly int[]? ids;

	internal EntityIdFilter(string filterText, MatchType matchType, int id, int[]? ids)
		: base(filterText)
	{
		this.matchType = matchType;
		this.id = id;
		this.ids = ids;
	}

	internal override bool FilterField(Entity entity)
	{
		int num = entity.Id;
		int num2 = id;
		return matchType switch
		{
			MatchType.LessThan => num < num2, 
			MatchType.LessThanOrEqual => num <= num2, 
			MatchType.GreaterThan => num > num2, 
			MatchType.GreaterThanOrEqual => num >= num2, 
			MatchType.Equal => num == num2, 
			MatchType.NotEqual => num != num2, 
			MatchType.In => Array.IndexOf(ids, num) >= 0, 
			_ => throw new InvalidOperationException($"unsupported match type {matchType}"), 
		};
	}
}
