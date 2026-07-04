using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal struct InputIdentifier
{
	internal DrawContext context;

	internal Entity entity;

	internal MemberPath memberPath;

	internal InputIdentifier(in DrawValue drawValue)
	{
		context = drawValue.context;
		entity = drawValue.context.entity;
		memberPath = drawValue.memberDrawer.member;
	}

	internal void Set(in DrawValue drawValue)
	{
		context = drawValue.context;
		entity = drawValue.context.entity;
		memberPath = drawValue.memberDrawer.member;
	}

	public bool IsEqual(InputIdentifier other)
	{
		if (entity == other.entity && memberPath == other.memberPath)
		{
			return context == other.context;
		}
		return false;
	}
}
