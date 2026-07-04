using System.Runtime.CompilerServices;

namespace Friflo.EcGui;

[InlineArray(1024)]
internal struct HistoryArray<TMember> where TMember : struct
{
	internal TMember values;
}
