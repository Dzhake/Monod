using System;
using System.Collections.Generic;
using Friflo.EcGui.Friflo.EcGui;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class RelationContainer<TRelation, TKey> : IContainer, IDisposable where TRelation : struct, IRelation<TKey>
{
	private int count;

	private Relations<TRelation> relations;

	private RelationsEnumerator<TRelation> enumerator;

	private int index;

	private DrawValue drawValue;

	private static readonly Stack<RelationContainer<TRelation, TKey>> Pool = new Stack<RelationContainer<TRelation, TKey>>();

	public int Count => count;

	public bool IsNull => ItemUtils.IsNull(relations);

	public Type ItemType => typeof(TKey);

	public TRelation Current => enumerator.Current;

	public override string ToString()
	{
		return $"index: {index}  count: {count}";
	}

	private RelationContainer()
	{
	}

	internal static IContainer Get(Relations<TRelation> relations, in DrawValue drawValue)
	{
		if (!Pool.TryPop(out RelationContainer<TRelation, TKey> result))
		{
			result = new RelationContainer<TRelation, TKey>();
		}
		result.drawValue = drawValue;
		result.count = relations.Length;
		result.relations = relations;
		return result;
	}

	public void Dispose()
	{
		count = -1;
		relations = default(Relations<TRelation>);
		enumerator = default(RelationsEnumerator<TRelation>);
		index = -1;
		Pool.Push(this);
	}

	public void StartIterator()
	{
		index = -1;
		enumerator = relations.GetEnumerator();
	}

	public bool MoveNext()
	{
		index++;
		return enumerator.MoveNext();
	}

	public void SeekCurrent(int offset)
	{
		for (int i = 0; i < offset; i++)
		{
			if (!enumerator.MoveNext())
			{
				break;
			}
			index++;
		}
	}

	public void Add(int index)
	{
		TRelation component = ItemUtils.CreateItem<TRelation>();
		TKey relationKey = component.GetRelationKey();
		Entity entity = drawValue.Entity;
		if (entity.TryGetRelation<TRelation, TKey>(relationKey, out var _))
		{
			throw new ArgumentException($"Element '{relationKey}' is already in Set.");
		}
		TRelation[] array = EntityInspector.ClearRelations<TRelation, TKey>(drawValue.Entity);
		for (int i = 0; i < array.Length; i++)
		{
			if (i == index)
			{
				entity.AddRelation(in component);
			}
			entity.AddRelation(in array[i]);
		}
		if (index == array.Length)
		{
			entity.AddRelation(in component);
		}
	}

	public void Remove(int index)
	{
		TRelation[] array = EntityInspector.ClearRelations<TRelation, TKey>(drawValue.Entity);
		if (index == -1)
		{
			return;
		}
		int num = 0;
		TRelation[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			TRelation component = array2[i];
			if (num++ != index)
			{
				drawValue.Entity.AddRelation(in component);
			}
		}
	}

	internal void ChangeRelationItem(TRelation relation)
	{
		Entity entity = drawValue.Entity;
		TKey relationKey = relation.GetRelationKey();
		TRelation value = Current;
		TKey relationKey2 = value.GetRelationKey();
		if (entity.TryGetRelation<TRelation, TKey>(relationKey, out value) && !EqualityComparer<TKey>.Default.Equals(relationKey, relationKey2))
		{
			return;
		}
		TRelation[] array = EntityInspector.ClearRelations<TRelation, TKey>(drawValue.Entity);
		int num = 0;
		TRelation[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			TRelation component = array2[i];
			if (num++ == index)
			{
				entity.AddRelation(in relation);
			}
			else
			{
				drawValue.Entity.AddRelation(in component);
			}
		}
		enumerator = relations.GetEnumerator();
		for (int j = 0; j <= index; j++)
		{
			enumerator.MoveNext();
		}
	}
}
