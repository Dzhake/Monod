namespace Friflo.EcGui;

internal struct FieldHistory<TMember> where TMember : struct
{
	internal HistoryArray<TMember> array;

	internal int lastSample;
}
