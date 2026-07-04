using System.Linq.Expressions;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal abstract class FieldFilter
{
	private readonly string filterText;

	public override string ToString()
	{
		return filterText;
	}

	internal abstract bool FilterField(Entity entity);

	internal FieldFilter(string filterText)
	{
		this.filterText = filterText;
	}
}
internal sealed class FieldFilter<TComponent> : FieldFilter where TComponent : struct, IComponent
{
	private readonly ComponentTypes types;

	private readonly ComponentFieldFilter<TComponent> componentFilter;

	private readonly MatchInfo matchInfo;

	public override string ToString()
	{
		return matchInfo.ToString();
	}

	private FieldFilter(string filterText, ComponentType componentType, ComponentFieldFilter<TComponent> componentFilter, MatchInfo matchInfo)
		: base(filterText)
	{
		types = new ComponentTypes(componentType);
		this.componentFilter = componentFilter;
		this.matchInfo = matchInfo;
	}

	internal static FieldFilter CreateFilterLambda(string filterText, ComponentType componentType, BinaryExpression matchExpr, ParameterExpression arg, MatchInfo matchInfo)
	{
		ComponentFieldFilter<TComponent> componentFieldFilter = Expression.Lambda<ComponentFieldFilter<TComponent>>(matchExpr, new ParameterExpression[1] { arg }).Compile();
		return new FieldFilter<TComponent>(filterText, componentType, componentFieldFilter, matchInfo);
	}

	internal override bool FilterField(Entity entity)
	{
		if (!entity.Archetype.ComponentTypes.HasAll(in types))
		{
			return false;
		}
		return componentFilter(in entity.GetComponent<TComponent>());
	}
}
