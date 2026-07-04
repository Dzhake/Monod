using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Friflo.EcGui;

internal sealed class QuaternionDrawer : TypeDrawer
{
	public override int DefaultWidth => 500;

	public override string[] SortFields => new string[4] { "X", "Y", "Z", "W" };

	public override string[] FormatFields => new string[4] { "X", "Y", "Z", "W" };

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<Quaternion>(out Quaternion value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		Vector4 source = Unsafe.As<Quaternion, Vector4>(ref value);
		if (EcUtils.InputFloat4(ref source.X, ref source.Y, ref source.Z, ref source.W, in drawValue, out var flags))
		{
			value = Unsafe.As<Vector4, Quaternion>(ref source);
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
