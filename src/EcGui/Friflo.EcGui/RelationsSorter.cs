using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal abstract class RelationsSorter
{
	internal static int[] indexMap = Array.Empty<int>();

	internal abstract void AddRelations(EntityList entities, RelationEntry[] entries);
}
internal class RelationsSorter<TRelation> : RelationsSorter where TRelation : struct, IRelation
{
	internal override void AddRelations(EntityList entities, RelationEntry[] entries)
	{
		int num = 0;
		foreach (Entity entity in entities)
		{
			ref RelationEntry reference = ref entries[num++];
			reference.id = entity.Id;
			reference.count = ((!entity.IsNull) ? entity.GetRelations<TRelation>().Length : 0);
		}
	}

	internal void AddRelationsWip(EntityList entities, RelationEntry[] entries)
	{
		EntityStore entityStore = entities.EntityStore;
		int num = entityStore.NodeMaxId + 1;
		if (RelationsSorter.indexMap.Length < num)
		{
			RelationsSorter.indexMap = new int[num];
		}
		int[] array = RelationsSorter.indexMap;
		for (int i = 0; i < entries.Length; i++)
		{
			int id = entities[i].Id;
			ref RelationEntry reference = ref entries[i];
			reference.id = id;
			reference.count = 0;
			array[id] = i;
		}
		foreach (Entity allEntitiesWithRelation in entityStore.GetAllEntitiesWithRelations<TRelation>())
		{
			entries[array[allEntitiesWithRelation.Id]].count = allEntitiesWithRelation.GetRelations<TRelation>().Length;
		}
	}
}
