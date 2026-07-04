using System;

namespace Friflo.EcGui;

internal sealed class ClassDrawer<T> : TypeDrawer, IClassDrawer, IObjectDrawer
{
	internal ClassDrawer()
	{
	}

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		return ObjectDrawerUtils.DrawType(in drawValue, typeof(T));
	}

	public object? GetObject(in DrawValue drawValue, out Exception? exception)
	{
		drawValue.GetValue<T>(out T value, out exception);
		return value;
	}

	object? IClassDrawer.GetObject(in DrawValue drawValue, out Exception? exception)
	{
		return GetObject(in drawValue, out exception);
	}
}
