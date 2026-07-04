using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal readonly struct HistoryKey : IEquatable<HistoryKey>
{
	private readonly EntityStore store;

	private readonly MemberPath member;

	internal HistoryKey(EntityStore store, MemberPath member)
	{
		this.store = store;
		this.member = member;
	}

	public override int GetHashCode()
	{
		return store.GetHashCode() ^ member.GetHashCode();
	}

	public bool Equals(HistoryKey other)
	{
		if (store.Equals(other.store))
		{
			return member.Equals(other.member);
		}
		return false;
	}
}
