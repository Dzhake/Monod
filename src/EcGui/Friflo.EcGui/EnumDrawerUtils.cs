using System;
using System.Runtime.CompilerServices;

namespace Friflo.EcGui;

internal static class EnumDrawerUtils
{
	internal static long GetValue_SByte<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, sbyte>(ref value);
	}

	internal static long GetValue_Int16<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, short>(ref value);
	}

	internal static long GetValue_Int32<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, int>(ref value);
	}

	internal static long GetValue_Int64<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, long>(ref value);
	}

	internal static long GetValue_Byte<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, byte>(ref value);
	}

	internal static long GetValue_UInt16<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, ushort>(ref value);
	}

	internal static long GetValue_UInt32<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return Unsafe.As<T, uint>(ref value);
	}

	internal static long GetValue_UInt64<T>(in DrawValue drawValue, out Exception? e)
	{
		drawValue.GetValue<T>(out T value, out e);
		return (long)Unsafe.As<T, ulong>(ref value);
	}

	internal static void SetValue_SByte<T>(in DrawValue drawValue, long l)
	{
		sbyte source = (sbyte)l;
		drawValue.SetValue(Unsafe.As<sbyte, T>(ref source));
	}

	internal static void SetValue_Int16<T>(in DrawValue drawValue, long l)
	{
		short source = (short)l;
		drawValue.SetValue(Unsafe.As<short, T>(ref source));
	}

	internal static void SetValue_Int32<T>(in DrawValue drawValue, long l)
	{
		int source = (int)l;
		drawValue.SetValue(Unsafe.As<int, T>(ref source));
	}

	internal static void SetValue_Int64<T>(in DrawValue drawValue, long l)
	{
		long source = l;
		drawValue.SetValue(Unsafe.As<long, T>(ref source));
	}

	internal static void SetValue_Byte<T>(in DrawValue drawValue, long l)
	{
		byte source = (byte)l;
		drawValue.SetValue(Unsafe.As<byte, T>(ref source));
	}

	internal static void SetValue_UInt16<T>(in DrawValue drawValue, long l)
	{
		ushort source = (ushort)l;
		drawValue.SetValue(Unsafe.As<ushort, T>(ref source));
	}

	internal static void SetValue_UInt32<T>(in DrawValue drawValue, long l)
	{
		uint source = (uint)l;
		drawValue.SetValue(Unsafe.As<uint, T>(ref source));
	}

	internal static void SetValue_UInt64<T>(in DrawValue drawValue, long l)
	{
		ulong source = (ulong)l;
		drawValue.SetValue(Unsafe.As<ulong, T>(ref source));
	}
}
