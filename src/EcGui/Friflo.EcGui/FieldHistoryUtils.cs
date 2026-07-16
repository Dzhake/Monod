using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Friflo.EcGui;

internal static class FieldHistoryUtils
{
	internal static MethodInfo GetCopyHistoryMethod(Type historyType, Type fieldType)
	{
		Type[] types = new Type[3]
		{
			historyType.MakeByRefType(),
			typeof(float[]),
			typeof(int)
		};
		if (fieldType.IsEnum)
		{
			Type underlyingType = Enum.GetUnderlyingType(fieldType);
			string name = "CopyEnumHistory_" + Type.GetTypeCode(underlyingType);
			return typeof(FieldHistoryUtils).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod).MakeGenericMethod(fieldType);
		}
		MethodInfo? method = typeof(FieldHistoryUtils).GetMethod("CopyHistory", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, types);
		if (method == null)
		{
			throw new InvalidOperationException($"Unsupported history Type: {fieldType}");
		}
		return method;
	}

	private static void CopyHistory(in HistoryArray<sbyte> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<short> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<int> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<long> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<byte> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<ushort> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<uint> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<ulong> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<float> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<double> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = (float)source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<decimal> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = (float)source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<char> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) % 1024] = source[i];
		}
	}

	private static void CopyHistory(in HistoryArray<bool> source, float[] target, int start)
	{
		for (int i = 0; i < 1024; i++)
		{
			target[(i + start) & 1023] = source[i] ? 1 : 0;
		}
	}

	private static void CopyEnumHistory_SByte<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<sbyte>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_Int16<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<short>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_Int32<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<int>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_Int64<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<long>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_Byte<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<byte>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_UInt16<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<ushort>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_UInt32<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<uint>>(ref Unsafe.AsRef(ref source)), target, start);
	}

	private static void CopyEnumHistory_UInt64<TEnum>(ref HistoryArray<TEnum> source, float[] target, int start) where TEnum : struct
	{
		CopyHistory(in Unsafe.As<HistoryArray<TEnum>, HistoryArray<ulong>>(ref Unsafe.AsRef(ref source)), target, start);
	}
}
