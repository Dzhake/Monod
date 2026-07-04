using System;

namespace Friflo.EcGui;

internal readonly struct EnumBitValue
{
	internal readonly long value;

	internal readonly int index;

	internal readonly EnumName name;

	internal Span<char> Name => name.Name;

	public override string ToString()
	{
		return $"{Name} ({index})";
	}

	internal EnumBitValue(long value, int index, EnumName name)
	{
		this.value = value;
		this.index = index;
		this.name = name;
	}
}
