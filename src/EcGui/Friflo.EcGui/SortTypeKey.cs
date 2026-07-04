using System;

namespace Friflo.EcGui;

internal readonly struct SortTypeKey : IEquatable<SortTypeKey>
{
	private readonly Type componentType;

	private readonly Type fieldType;

	internal SortTypeKey(Type componentType, Type fieldType)
	{
		this.componentType = componentType;
		this.fieldType = fieldType;
	}

	public bool Equals(SortTypeKey other)
	{
		if (componentType == other.componentType)
		{
			return fieldType == other.fieldType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(componentType, fieldType);
	}
}
