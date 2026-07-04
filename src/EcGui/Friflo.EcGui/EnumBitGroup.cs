namespace Friflo.EcGui;

internal readonly struct EnumBitGroup
{
	internal readonly EnumBitValue[] bitValues;

	internal EnumBitGroup(EnumBitValue[] bitValues)
	{
		this.bitValues = bitValues;
	}
}
