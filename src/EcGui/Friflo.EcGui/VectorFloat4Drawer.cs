using System;
using System.Numerics;

namespace Friflo.EcGui;

internal sealed class VectorFloat4Drawer : TypeDrawer
{
	public override int DefaultWidth => 500;

	public override string[] SortFields => new string[4] { "X", "Y", "Z", "W" };

	public override string[] FormatFields => new string[4] { "X", "Y", "Z", "W" };

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<Vector4>(out Vector4 value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.InputFloat4(ref value.X, ref value.Y, ref value.Z, ref value.W, in drawValue, out var flags))
		{
			drawValue.SetValue(value);
		}
		return flags;
	}

	public override void Format(MemberFormat format)
	{
		format.GetValue<Vector4>(out Vector4 value, out Exception exception);
		format.Append(value.X, exception);
		format.Append(value.Y, exception);
		format.Append(value.Z, exception);
		format.Append(value.W, exception);
	}
}
