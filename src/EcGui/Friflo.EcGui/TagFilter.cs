using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class TagFilter : FieldFilter
{
	private readonly Tags tag;

	private readonly bool hasTag;

	internal TagFilter(string filterText, TagType tagType, bool hasTag)
		: base(filterText)
	{
		tag = new Tags(tagType);
		this.hasTag = hasTag;
	}

	internal override bool FilterField(Entity entity)
	{
		return entity.Tags.HasAll(in tag) == hasTag;
	}
}
