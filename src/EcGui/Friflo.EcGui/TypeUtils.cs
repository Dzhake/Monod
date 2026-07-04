using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Friflo.EcGui;

internal static class TypeUtils
{
	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	internal static bool HasDefaultConstructor(Type type)
	{
		if (type.IsValueType || type.IsArray)
		{
			return true;
		}
		return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null;
	}

	internal static object CreateInstance(Type type)
	{
		if (type.IsValueType)
		{
			return Activator.CreateInstance(type);
		}
		if (type.IsArray)
		{
			return Array.CreateInstance(type.GetElementType(), 0);
		}
		return Activator.CreateInstance(type, nonPublic: true);
	}

	internal static T CreateInstance<T>()
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsValueType)
		{
			return Activator.CreateInstance<T>();
		}
		if (typeFromHandle.IsArray)
		{
			return (T)(object)Array.CreateInstance(typeFromHandle.GetElementType(), 0);
		}
		return (T)Activator.CreateInstance(typeFromHandle, nonPublic: true);
	}

	internal static string GetTypeName(Type type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendTypeName(stringBuilder, type);
		return stringBuilder.ToString();
	}

	internal static void AppendTypeName(StringBuilder sb, Type type)
	{
		if (type.IsEnum)
		{
			Type underlyingType = Enum.GetUnderlyingType(type);
			sb.Append(type.Name);
			sb.Append(" (");
			AppendName(sb, underlyingType);
			sb.Append(')');
		}
		else
		{
			AppendName(sb, type);
		}
	}

	private static void AppendName(StringBuilder sb, Type type)
	{
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.String:
			sb.Append("string");
			return;
		case TypeCode.Char:
			sb.Append("char");
			return;
		case TypeCode.Int64:
			sb.Append("long");
			return;
		case TypeCode.Int32:
			sb.Append("int");
			return;
		case TypeCode.Int16:
			sb.Append("short");
			return;
		case TypeCode.SByte:
			sb.Append("sbyte");
			return;
		case TypeCode.UInt64:
			sb.Append("ulong");
			return;
		case TypeCode.UInt32:
			sb.Append("uint");
			return;
		case TypeCode.UInt16:
			sb.Append("ushort");
			return;
		case TypeCode.Byte:
			sb.Append("byte");
			return;
		case TypeCode.Boolean:
			sb.Append("bool");
			return;
		case TypeCode.Single:
			sb.Append("float");
			return;
		case TypeCode.Double:
			sb.Append("double");
			return;
		case TypeCode.Decimal:
			sb.Append("decimal");
			return;
		case TypeCode.DateTime:
			sb.Append("DateTime");
			return;
		}
		string name = type.Name;
		if (!type.IsGenericType)
		{
			sb.Append(name);
			return;
		}
		int num = name.IndexOf('`');
		if (num == -1)
		{
			sb.Append(name);
		}
		else
		{
			sb.Append(name, 0, num);
		}
		sb.Append('<');
		Type[] genericArguments = type.GetGenericArguments();
		foreach (Type type2 in genericArguments)
		{
			AppendName(sb, type2);
			sb.Append(", ");
		}
		sb.Length -= 2;
		sb.Append('>');
	}

	internal static bool ParseValue(Type type, string stringValue, out object? result)
	{
		if (type.IsEnum)
		{
			return Enum.TryParse(type, stringValue, out result);
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.String:
			result = stringValue;
			return true;
		case TypeCode.Char:
		{
			char result20;
			bool result19 = char.TryParse(stringValue, out result20);
			result = result20;
			return result19;
		}
		case TypeCode.Int64:
		{
			long result18;
			bool result17 = long.TryParse(stringValue, out result18);
			result = result18;
			return result17;
		}
		case TypeCode.Int32:
		{
			int result16;
			bool result15 = int.TryParse(stringValue, out result16);
			result = result16;
			return result15;
		}
		case TypeCode.Int16:
		{
			short result14;
			bool result13 = short.TryParse(stringValue, out result14);
			result = result14;
			return result13;
		}
		case TypeCode.SByte:
		{
			sbyte result12;
			bool result11 = sbyte.TryParse(stringValue, out result12);
			result = result12;
			return result11;
		}
		case TypeCode.UInt64:
		{
			ulong result28;
			bool result27 = ulong.TryParse(stringValue, out result28);
			result = result28;
			return result27;
		}
		case TypeCode.UInt32:
		{
			uint result26;
			bool result25 = uint.TryParse(stringValue, out result26);
			result = result26;
			return result25;
		}
		case TypeCode.UInt16:
		{
			ushort result24;
			bool result23 = ushort.TryParse(stringValue, out result24);
			result = result24;
			return result23;
		}
		case TypeCode.Byte:
		{
			byte result22;
			bool result21 = byte.TryParse(stringValue, out result22);
			result = result22;
			return result21;
		}
		case TypeCode.Boolean:
		{
			bool result10;
			bool flag = bool.TryParse(stringValue, out result10);
			if (flag)
			{
				result = result10;
				return true;
			}
			if (stringValue == "0")
			{
				result = false;
				return true;
			}
			if (stringValue == "1")
			{
				result = true;
				return true;
			}
			result = false;
			return flag;
		}
		case TypeCode.Single:
		{
			float result9;
			bool result8 = float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result9);
			result = result9;
			return result8;
		}
		case TypeCode.Double:
		{
			double result7;
			bool result6 = double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result7);
			result = result7;
			return result6;
		}
		case TypeCode.Decimal:
		{
			decimal result5;
			bool result4 = decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out result5);
			result = result5;
			return result4;
		}
		case TypeCode.DateTime:
		{
			DateTime result3;
			bool result2 = DateTime.TryParse(stringValue, out result3);
			result3 = result3.ToUniversalTime();
			result = result3;
			return result2;
		}
		default:
			result = null;
			return false;
		}
	}
}
