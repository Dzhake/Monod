using System;

namespace Friflo.EcGui;

internal readonly struct EnumName
{
	private readonly char[] name;

	private readonly char[]? uiFlag;

	internal Span<char> Name => name.AsSpan();

	internal Span<char> UiFlag => uiFlag.AsSpan();

	public override string? ToString()
	{
		return name.ToString();
	}

	internal EnumName(string name, string? uiFlag)
	{
		this.name = name.ToCharArray();
		this.uiFlag = uiFlag?.ToCharArray();
	}
}
