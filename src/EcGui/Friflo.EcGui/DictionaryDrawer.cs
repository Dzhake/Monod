using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class DictionaryDrawer<TDictionary, TKey, TValue> : ContainerDrawer where TDictionary : IDictionary<TKey, TValue> where TKey : notnull
{
	private ItemMember[]? itemMembers;

	public override string[] SortFields => new string[1] { "Count" };

	internal static TypeDrawer CreateDictionaryDrawer()
	{
		return new DictionaryDrawer<TDictionary, TKey, TValue>();
	}

	protected override ItemMember[] GetItemMembers()
	{
		return itemMembers ?? (itemMembers = ContainerDrawer.Cap(DictionaryDrawerUtils.CreateItemMembers(typeof(DictionaryDrawer<, , >), typeof(TDictionary), typeof(TKey), typeof(TValue))));
	}

	internal static bool GetMapKeyMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		DictionaryContainer<TDictionary, TKey, TValue> dictionaryContainer = (DictionaryContainer<TDictionary, TKey, TValue>)drawValue.container;
		try
		{
			exception = null;
			value = (TMember)(object)dictionaryContainer.Current.Key;
			return true;
		}
		catch (Exception ex)
		{
			value = default(TMember);
			exception = ex;
			return false;
		}
	}

	internal static bool SetMapKeyMember<TMember>(in DrawValue drawValue, in TMember value, out Exception exception)
	{
		DictionaryContainer<TDictionary, TKey, TValue> dictionaryContainer = (DictionaryContainer<TDictionary, TKey, TValue>)drawValue.container;
		try
		{
			exception = null;
			dictionaryContainer.ChangeDictionaryKey((TKey)(object)value);
			return true;
		}
		catch (Exception ex)
		{
			exception = ex;
			return false;
		}
	}

	internal static bool GetMapValueMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception)
	{
		DictionaryContainer<TDictionary, TKey, TValue> dictionaryContainer = (DictionaryContainer<TDictionary, TKey, TValue>)drawValue.container;
		MemberPathGetter<TValue, TMember> memberPathGetter = (MemberPathGetter<TValue, TMember>)drawValue.memberDrawer.member.getter;
		try
		{
			exception = null;
			value = memberPathGetter(dictionaryContainer.Current.Value);
			return true;
		}
		catch (Exception ex)
		{
			value = default(TMember);
			exception = ex;
			return false;
		}
	}

	internal static bool SetMapValueMember<TMember>(in DrawValue drawValue, in TMember value, out Exception exception)
	{
		DictionaryContainer<TDictionary, TKey, TValue> dictionaryContainer = (DictionaryContainer<TDictionary, TKey, TValue>)drawValue.container;
		MemberPathSetter<TValue, TMember> memberPathSetter = (MemberPathSetter<TValue, TMember>)drawValue.memberDrawer.member.setter;
		try
		{
			exception = null;
			TValue root = dictionaryContainer.Current.Value;
			memberPathSetter(ref root, value);
			dictionaryContainer.ChangeDictionaryValue(root);
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
		if (!drawValue.GetValue<TDictionary>(out TDictionary value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		using IContainer container = DictionaryContainer<TDictionary, TKey, TValue>.Get(value, in drawValue);
		return DrawContainer.Draw(container, array, in drawValue);
	}
}
