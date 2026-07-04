using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal struct ContextMenu
{
	internal QueryExplorer explorer;

	internal Column column;

	internal Entity entity;

	internal int rowIndex;

	internal int columnIndex;
}
