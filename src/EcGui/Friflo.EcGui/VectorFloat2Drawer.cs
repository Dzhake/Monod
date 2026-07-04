using System;
using System.Numerics;

namespace Friflo.EcGui;

internal sealed class VectorFloat2Drawer : TypeDrawer
{
	public override int DefaultWidth => 300;

	public override string[] SortFields => new string[2] { "X", "Y" };

	public override string[] FormatFields => new string[2] { "X", "Y" };

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<Vector2>(out Vector2 value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		if (EcUtils.InputFloat2(ref value.X, ref value.Y, in drawValue, out var flags))
		{
			drawValue.SetValue(value);
		}
		return flags;
	}

	public override void Format(MemberFormat format)
	{
		format.GetValue<Vector2>(out Vector2 value, out Exception exception);
		format.Append(value.X, exception);
		format.Append(value.Y, exception);
	}
}
