using System;
using System.Numerics;

namespace Friflo.EcGui;

internal sealed class Color3Drawer : TypeDrawer
{
	public override int DefaultWidth => 280;

	public override string[] SortFields => new string[3] { "X", "Y", "Z" };

	public override string[] FormatFields => new string[3] { "R", "G", "B" };

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<Vector3>(out Vector3 value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.ColorEdit3(ref value.X, ref value.Y, ref value.Z, in drawValue, out var flags))
		{
			drawValue.SetValue(value);
		}
		return flags;
	}

	public override void Format(MemberFormat format)
	{
		format.GetValue<Vector3>(out Vector3 value, out Exception exception);
		format.Append(value.X, exception);
		format.Append(value.Y, exception);
		format.Append(value.Z, exception);
	}
}
