namespace Friflo.EcGui;

internal delegate void CopyHistory<TMember>(in HistoryArray<TMember> fieldHistory, float[] target, int start) where TMember : struct;
