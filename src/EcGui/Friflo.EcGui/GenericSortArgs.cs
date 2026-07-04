using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class GenericSortArgs
{
	internal EntityList entities = new EntityList();

	internal string fieldPath = "";

	internal SortOrder sortOrder;
}
