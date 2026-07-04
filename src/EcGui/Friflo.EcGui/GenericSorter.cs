using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class GenericSorter<TComponent, TMember> where TComponent : struct, IComponent
{
	private static Array _sortBuffer = Array.Empty<object>();

	internal static void Sort(GenericSortArgs args)
	{
		ComponentField<TMember>[] fields = ((!(_sortBuffer.GetType() == typeof(ComponentField<TMember>[]))) ? new ComponentField<TMember>[args.entities.Count] : ((ComponentField<TMember>[])_sortBuffer));
		_sortBuffer = args.entities.SortByComponentField<TComponent, TMember>(args.fieldPath, args.sortOrder, fields);
	}
}
