using System;
using System.Runtime.CompilerServices;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal abstract class RelationsDrawer : ContainerDrawer
{
	internal abstract Type RelationType { get; }

	internal abstract bool GetEntityRelations<TValue>(Entity entity, out TValue value);
}
internal sealed class RelationsDrawer<TRelation, TKey> : RelationsDrawer where TRelation : struct, IRelation<TKey>
{
	private ItemMember[]? itemMembers;

	internal override Type RelationType => typeof(TRelation);

	internal override bool GetEntityRelations<TValue>(Entity entity, out TValue value)
	{
		Relations<TRelation> source = entity.GetRelations<TRelation>();
		value = Unsafe.As<Relations<TRelation>, TValue>(ref source);
		return true;
	}

	internal static TypeDrawer CreateRelationDrawer()
	{
		return new RelationsDrawer<TRelation, TKey>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(RelationsDrawerUtils.CreateItemMembers(typeof(RelationsDrawer<, >), typeof(TRelation), typeof(TKey))));
	}

	internal static bool GetListItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		RelationContainer<TRelation, TKey> relationContainer = (RelationContainer<TRelation, TKey>)drawValue.container;
		MemberPathGetter<TRelation, TMember> memberPathGetter = (MemberPathGetter<TRelation, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(relationContainer.Current);
			return true;
		}
		catch (Exception ex)
		{
			value = default(TMember);
			exception = ex;
			return false;
		}
	}

	internal static bool SetListItemMember<TMember>(in DrawValue drawValue, in TMember value, out Exception exception)
	{
		RelationContainer<TRelation, TKey> relationContainer = (RelationContainer<TRelation, TKey>)drawValue.container;
		MemberPathSetter<TRelation, TMember> memberPathSetter = (MemberPathSetter<TRelation, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TRelation root = relationContainer.Current;
			memberPathSetter(ref root, value);
			relationContainer.ChangeRelationItem(root);
			return true;
		}
		catch (Exception ex)
		{
			exception = ex;
			return false;
		}
	}

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		ItemMember[] array = GetItemMembers();
		if (!drawValue.GetValue<Relations<TRelation>>(out Relations<TRelation> value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = RelationContainer<TRelation, TKey>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
