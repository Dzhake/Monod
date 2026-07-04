using System;

namespace Friflo.EcGui;

internal sealed class StructDrawer : TypeDrawer, IObjectDrawer
{
	private readonly Type type;

	internal StructDrawer(Type type)
	{
		this.type = type;
	}

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		return ObjectDrawerUtils.DrawType(in drawValue, type);
	}
}
