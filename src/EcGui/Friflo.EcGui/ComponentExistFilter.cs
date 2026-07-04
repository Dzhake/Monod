using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class ComponentExistFilter : FieldFilter
{
	private readonly ComponentTypes types;

	private readonly bool exist;

	internal ComponentExistFilter(string filterText, ComponentType componentType, bool exist)
		: base(filterText)
	{
		types = new ComponentTypes(componentType);
		this.exist = exist;
	}

	internal override bool FilterField(Entity entity)
	{
		return entity.Archetype.ComponentTypes.HasAll(in types) == exist;
	}
}
