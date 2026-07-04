using System.Numerics;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal struct DrawCell
{
	internal DrawContext context;

	internal bool selected;

	internal QueryExplorer explorer;

	internal DrawValueFlags drawValueFlags;

	internal Entity Entity => context.entity;

	internal Vector2 Size => context.rect.size;
}
