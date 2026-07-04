using System;

namespace Friflo.EcGui;

internal interface IClassDrawer
{
	object? GetObject(in DrawValue drawValue, out Exception? exception);
}
