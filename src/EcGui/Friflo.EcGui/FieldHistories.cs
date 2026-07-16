using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal abstract class FieldHistories
{
	internal readonly MemberPath member;

	private int subscriptionCount;

	internal int sampleIndex;

	internal bool allFields;

	internal static int globalSampleIndex;

	private static readonly Dictionary<HistoryKey, FieldHistories> Map = new Dictionary<HistoryKey, FieldHistories>();

	internal const int MaxSamples = 1024;

	protected abstract void AddHistorySamples();

	internal abstract int GetHistory(Entity entity, float[] target, int start);

	public abstract bool HasHistory(Entity entity);

	internal abstract void SelectEntity(Entity entity, bool select);

	internal FieldHistories(MemberPath member)
	{
		this.member = member;
		subscriptionCount = 1;
	}

	internal static void AddHistories()
	{
		globalSampleIndex++;
		foreach (KeyValuePair<HistoryKey, FieldHistories> item in Map)
		{
			item.Deconstruct(out var _, out var value);
			value.AddHistorySamples();
		}
	}

	internal static FieldHistories SubscribeHistory(EntityStore store, MemberPath member)
	{
		HistoryKey key = new HistoryKey(store, member);
		if (Map.TryGetValue(key, out FieldHistories value))
		{
			value.subscriptionCount++;
			return value;
		}
		Type type = member.componentType.Type;
		Type memberType = member.memberType;
		FieldHistories fieldHistories = (FieldHistories)typeof(FieldHistories<, >).MakeGenericType(type, memberType).GetMethod("CreateHistory", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[2] { store, member.path });
		Map.Add(key, fieldHistories);
		return fieldHistories;
	}

	internal static void ReleaseSubscription(FieldHistories histories)
	{
		foreach (var (key, fieldHistories2) in Map)
		{
			if (fieldHistories2 == histories)
			{
				histories.subscriptionCount--;
				if (histories.subscriptionCount == 0)
				{
					Map.Remove(key);
				}
				break;
			}
		}
	}
}
internal sealed class FieldHistories<TComponent, TMember> : FieldHistories where TComponent : struct, IComponent where TMember : struct
{
	private readonly MemberPathGetter<TComponent, TMember> fieldGetter;

	private readonly ArchetypeQuery<TComponent> query;

	private readonly CopyHistory<TMember> copyHistory;

	private readonly Dictionary<RawEntity, FieldHistory<TMember>> allHistories = new Dictionary<RawEntity, FieldHistory<TMember>>();

	private readonly Dictionary<RawEntity, FieldHistory<TMember>> selectedHistories = new Dictionary<RawEntity, FieldHistory<TMember>>();

	private FieldHistories(ArchetypeQuery<TComponent> query, MemberPathGetter<TComponent, TMember> fieldGetter, CopyHistory<TMember> copyHistory, MemberPath memberPath)
		: base(memberPath)
	{
		this.query = query;
		this.fieldGetter = fieldGetter;
		this.copyHistory = copyHistory;
	}

	internal static FieldHistories CreateHistory(EntityStore store, string memberName)
	{
		MemberPath memberPath = MemberPath.Get(typeof(TComponent), memberName);
		ArchetypeQuery<TComponent> archetypeQuery = store.Query<TComponent>();
		MemberPathGetter<TComponent, TMember> memberPathGetter = (MemberPathGetter<TComponent, TMember>)memberPath.getter;
		CopyHistory<TMember> copyHistory = (CopyHistory<TMember>)Delegate.CreateDelegate(method: FieldHistoryUtils.GetCopyHistoryMethod(typeof(HistoryArray<TMember>), typeof(TMember)), type: typeof(CopyHistory<TMember>));
		return new FieldHistories<TComponent, TMember>(archetypeQuery, memberPathGetter, copyHistory, memberPath);
	}

	internal override void SelectEntity(Entity entity, bool select)
	{
		if (select)
		{
			selectedHistories[entity.RawEntity] = default(FieldHistory<TMember>);
		}
		else
		{
			selectedHistories.Remove(entity.RawEntity);
		}
	}

	public override bool HasHistory(Entity entity)
	{
		return selectedHistories.ContainsKey(entity.RawEntity);
	}

	protected override void AddHistorySamples()
	{
		int num = sampleIndex;
		int index = num % 1024;
		int num2 = num - 1024;
		MemberPathGetter<TComponent, TMember> memberPathGetter = fieldGetter;
		sampleIndex++;
		EntityStore store = query.Store;
		Dictionary<RawEntity, FieldHistory<TMember>> dictionary = selectedHistories;
		foreach (RawEntity key in dictionary.Keys)
		{
			bool exists;
			ref FieldHistory<TMember> valueRefOrAddDefault = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out exists);
			Entity entityByRawEntity = store.GetEntityByRawEntity(key);
			if (!entityByRawEntity.IsNull && entityByRawEntity.TryGetComponent<TComponent>(out var result))
			{
				TMember val;
				try
				{
					val = memberPathGetter(in result);
				}
				catch
				{
					val = default(TMember);
				}
				valueRefOrAddDefault.array[index] = val;
				valueRefOrAddDefault.lastSample = num;
			}
			else
			{
				valueRefOrAddDefault.array[index] = default(TMember);
			}
		}
		if (!allFields)
		{
			return;
		}
		Dictionary<RawEntity, FieldHistory<TMember>> dictionary2 = allHistories;
		foreach (RawEntity key2 in dictionary2.Keys)
		{
			if (dictionary2.ContainsKey(key2))
			{
				bool exists2;
				ref FieldHistory<TMember> valueRefOrAddDefault2 = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary2, key2, out exists2);
				if (valueRefOrAddDefault2.lastSample > num2)
				{
					valueRefOrAddDefault2.array[index] = default;
				}
				else
				{
					dictionary2.Remove(key2);
				}
			}
		}
		foreach (Chunks<TComponent> chunk3 in query.Chunks)
		{
			chunk3.Deconstruct(out var chunk, out var entities);
			Chunk<TComponent> chunk2 = chunk;
			ChunkEntities chunkEntities = entities;
			Span<TComponent> span = chunk2.Span;
			int length = chunk2.Length;
			for (int i = 0; i < length; i++)
			{
				RawEntity rawEntity = chunkEntities.EntityAt(i).RawEntity;
				bool exists3;
				ref FieldHistory<TMember> valueRefOrAddDefault3 = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary2, rawEntity, out exists3);
				TMember val2;
				try
				{
					val2 = memberPathGetter(in span[i]);
				}
				catch
				{
					val2 = default(TMember);
				}
				valueRefOrAddDefault3.array[index] = val2;
				valueRefOrAddDefault3.lastSample = num;
			}
		}
	}

	internal override int GetHistory(Entity entity, float[] target, int start)
	{
		Dictionary<RawEntity, FieldHistory<TMember>> dictionary = (allFields ? allHistories : selectedHistories);
		if (!dictionary.ContainsKey(entity.RawEntity))
		{
			if (!allFields)
			{
				return -1;
			}
			dictionary = selectedHistories;
			if (!dictionary.ContainsKey(entity.RawEntity))
			{
				return -1;
			}
		}
		bool exists;
		ref FieldHistory<TMember> valueRefOrAddDefault = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, entity.RawEntity, out exists);
		copyHistory(in valueRefOrAddDefault.array, target, start);
		return valueRefOrAddDefault.lastSample;
	}
}
