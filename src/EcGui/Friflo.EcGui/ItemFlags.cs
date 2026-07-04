using System;

namespace Friflo.EcGui;

[Flags]
public enum ItemFlags
{
	None = 0,
	Present = 1,
	Focused = 2,
	ContextMenu = 4
}
