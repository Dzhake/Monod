using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using Friflo.Engine.ECS;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class EnumDrawer : TypeDrawer
{
	private readonly Dictionary<long, EnumName> nameByValue = new Dictionary<long, EnumName>();

	private readonly bool isFlagsEnum;

	private readonly Type enumType;

	private readonly EnumBitGroup[] bitGroups = Array.Empty<EnumBitGroup>();

	private readonly int groupSpacing;

	private readonly GetEnumValue getValue;

	private readonly SetEnumValue setValue;

	internal readonly GetEnumObject getObject;

	public override int DefaultWidth
	{
		get
		{
			if (!isFlagsEnum)
			{
				return 200;
			}
			float num = ImGui.GetFrameHeight() / UI.scale;
			int num2 = 0;
			EnumBitGroup[] array = bitGroups;
			for (int i = 0; i < array.Length; i++)
			{
				EnumBitGroup enumBitGroup = array[i];
				num2 += (int)((float)enumBitGroup.bitValues.Length * num) + groupSpacing;
			}
			return num2;
		}
	}

	internal EnumDrawer(Type type)
	{
		enumType = type;
		isFlagsEnum = IsFagsEnum(type.CustomAttributes);
		TypeCode typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(type));
		Array values = Enum.GetValues(type);
		int length = values.Length;
		bool[] array = (isFlagsEnum ? new bool[64] : Array.Empty<bool>());
		List<EnumBitValue> list = new List<EnumBitValue>();
		nameByValue.EnsureCapacity(length);
		foreach (object item in values)
		{
			long num = Value2Long(typeCode, item);
			string name = item.ToString();
			string uiFlag = null;
			if (isFlagsEnum)
			{
				FieldInfo field = type.GetField(name);
				if (field != null)
				{
					uiFlag = GetUiFlag(field.CustomAttributes);
				}
			}
			EnumName enumName = new EnumName(name, uiFlag);
			nameByValue[num] = enumName;
			if (isFlagsEnum && BitOperations.PopCount((ulong)num) == 1)
			{
				int num2 = 63 - BitOperations.LeadingZeroCount((ulong)num);
				if (!array[num2])
				{
					array[num2] = true;
					list.Add(new EnumBitValue(num, num2, enumName));
				}
			}
		}
		if (isFlagsEnum)
		{
			bitGroups = CreateGroups(list, type, out groupSpacing);
		}
		BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.NonPublic;
		string name2 = "GetValue_" + typeCode;
		MethodInfo method = typeof(EnumDrawerUtils).GetMethod(name2, bindingAttr).MakeGenericMethod(type);
		getValue = (GetEnumValue)Delegate.CreateDelegate(typeof(GetEnumValue), method);
		string name3 = "SetValue_" + typeCode;
		MethodInfo method2 = typeof(EnumDrawerUtils).GetMethod(name3, bindingAttr).MakeGenericMethod(type);
		setValue = (SetEnumValue)Delegate.CreateDelegate(typeof(SetEnumValue), method2);
		getObject = CreateEnumObjectGetter(type);
	}

	private static long Value2Long(TypeCode typeCode, object value)
	{
		return typeCode switch
		{
			TypeCode.SByte => (sbyte)value, 
			TypeCode.Int16 => (short)value, 
			TypeCode.Int32 => (int)value, 
			TypeCode.Int64 => (long)value, 
			TypeCode.Byte => (byte)value, 
			TypeCode.UInt16 => (ushort)value, 
			TypeCode.UInt32 => (uint)value, 
			TypeCode.UInt64 => (long)(ulong)value, 
			_ => throw new InvalidOperationException("unsupported enum type"), 
		};
	}

	private static string? GetUiFlag(IEnumerable<CustomAttributeData> attributes)
	{
		foreach (CustomAttributeData attribute in attributes)
		{
			if (attribute.AttributeType == typeof(UiFlagAttribute))
			{
				return (string)attribute.ConstructorArguments[0].Value;
			}
		}
		return null;
	}

	private static bool IsFagsEnum(IEnumerable<CustomAttributeData> attributes)
	{
		foreach (CustomAttributeData attribute in attributes)
		{
			if (attribute.AttributeType == typeof(FlagsAttribute))
			{
				return true;
			}
		}
		return false;
	}

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		Exception exception;
		long num = getValue(in drawValue, out exception);
		if (exception != null)
		{
			return drawValue.DrawException(exception);
		}
		if (isFlagsEnum)
		{
			return DrawFlags(in drawValue, num);
		}
		EnumName value;
		bool num2 = ImGui.BeginCombo(preview_value: (!nameByValue.TryGetValue(num, out value)) ? TextUtils.LongAsBytes(num) : ((ReadOnlySpan<char>)value.Name), label: "##field", flags: ImGuiComboFlags.HeightLarge | ImGuiComboFlags.NoArrowButton);
		ItemFlags result = TypeDrawer.Flags();
		if (num2)
		{
			if (ImGui.IsWindowAppearing())
			{
				ImGui.SetKeyboardFocusHere();
			}
			foreach (KeyValuePair<long, EnumName> item in nameByValue)
			{
				item.Deconstruct(out var key, out var value2);
				long num3 = key;
				EnumName enumName = value2;
				bool selected = num3 == num;
				if (ImGui.Selectable(enumName.Name, selected))
				{
					setValue(in drawValue, num3);
				}
			}
			ImGui.EndCombo();
		}
		return result;
	}

	private static EnumBitGroup[] CreateGroups(List<EnumBitValue> bitValueList, Type type, out int groupSpacing)
	{
		if (bitValueList.Count == 0)
		{
			groupSpacing = 0;
			return Array.Empty<EnumBitGroup>();
		}
		int groupAttributes = GetGroupAttributes(type.CustomAttributes, out var ascending, out groupSpacing);
		groupAttributes = ((groupAttributes <= 0) ? 64 : groupAttributes);
		bitValueList.Sort((EnumBitValue a, EnumBitValue b) => a.index.CompareTo(b.index));
		List<EnumBitGroup> list = new List<EnumBitGroup>();
		int num = bitValueList[0].index / groupAttributes;
		List<EnumBitValue> list2 = new List<EnumBitValue>();
		foreach (EnumBitValue bitValue in bitValueList)
		{
			int num2 = bitValue.index / groupAttributes;
			if (num2 != num)
			{
				if (!ascending)
				{
					list2.Reverse();
				}
				list.Add(new EnumBitGroup(list2.ToArray()));
				list2.Clear();
				num = num2;
			}
			list2.Add(bitValue);
		}
		if (list2.Count > 0)
		{
			if (!ascending)
			{
				list2.Reverse();
			}
			list.Add(new EnumBitGroup(list2.ToArray()));
		}
		if (!ascending)
		{
			list.Reverse();
		}
		return list.ToArray();
	}

	private static int GetGroupAttributes(IEnumerable<CustomAttributeData> attributes, out bool ascending, out int groupSpacing)
	{
		foreach (CustomAttributeData attribute in attributes)
		{
			if (attribute.AttributeType == typeof(UiFlagsAttribute))
			{
				IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;
				ascending = (bool)constructorArguments[1].Value;
				groupSpacing = (int)constructorArguments[2].Value;
				return (int)constructorArguments[0].Value;
			}
		}
		ascending = false;
		groupSpacing = 10;
		return 8;
	}

	private ItemFlags DrawFlags(in DrawValue drawValue, long curValue)
	{
		ItemFlags itemFlags = ItemFlags.None;
		float num = 0f;
		float x = drawValue.Size.X;
		float frameHeight = ImGui.GetFrameHeight();
		Vector2 size = new Vector2(frameHeight, frameHeight);
		int num2 = (drawValue.MultiLine ? 1 : 0);
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, default(Vector2));
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0f, ImGui.GetStyle().FramePadding.Y));
		float num3 = num;
		for (int i = 0; i < bitGroups.Length; i++)
		{
			EnumBitGroup enumBitGroup = bitGroups[i];
			if (i > 0 && drawValue.MultiLine && num + (float)enumBitGroup.bitValues.Length * frameHeight > x)
			{
				ImGui.NewLine();
				num = 0f;
			}
			for (int j = 0; j < enumBitGroup.bitValues.Length; j++)
			{
				EnumBitValue enumBitValue = enumBitGroup.bitValues[j];
				if (j >= num2 && num + frameHeight > x)
				{
					DrawLeaveOut(drawValue.context.rect.left + num3);
					if (drawValue.MultiLine)
					{
						ImGui.NewLine();
					}
					else
					{
						i = bitGroups.Length;
					}
					break;
				}
				ImGui.SameLine(drawValue.context.rect.left + num);
				num += frameHeight;
				num3 = num;
				bool flag = (enumBitValue.value & curValue) != 0;
				ImGui.PushStyleColor(ImGuiCol.Button, flag ? GlobalColors.checkMark : GlobalColors.frameBg);
				ImGui.PushStyleColor(ImGuiCol.ButtonHovered, flag ? GlobalColors.flagHoveredSet : GlobalColors.flagHovered);
				ImGui.PushStyleColor(ImGuiCol.Text, flag ? GlobalColors.flagTextSet : GlobalColors.flagText);
				Span<char> uiFlag = enumBitValue.name.UiFlag;
				bool num4 = ImGui.Button(uiFlag.IsEmpty ? TextUtils.IntAsBytes(enumBitValue.index) : ((ReadOnlySpan<char>)uiFlag), size);
				ImGui.PopStyleColor(3);
				itemFlags |= TypeDrawer.Flags();
				if (ImGui.BeginItemTooltip())
				{
					StringBuilder stringBuilder = TextUtils.Clear();
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(15, 3, stringBuilder);
					handler.AppendFormatted((ReadOnlySpan<char>)enumBitValue.Name);
					handler.AppendLiteral("\nBit: ");
					handler.AppendFormatted(enumBitValue.index);
					handler.AppendLiteral("   Type: ");
					handler.AppendFormatted(enumType.Name);
					ImGui.Text(TextUtils.AsSpan(stringBuilder.Append(ref handler)));
					ImGui.EndTooltip();
				}
				if (num4)
				{
					long value = ((!flag) ? (curValue | enumBitValue.value) : (curValue & ~enumBitValue.value));
					setValue(in drawValue, value);
				}
			}
			num += UI.Scl(groupSpacing);
		}
		ImGui.PopStyleVar(3);
		return itemFlags;
	}

	private static void DrawLeaveOut(float left)
	{
		ImGui.SameLine(left);
		float x = UI.Scl(4f);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorPos = ImGui.GetCursorPos();
		Vector2 vector = ImGui.GetWindowPos() + new Vector2(cursorPos.X - ImGui.GetScrollX(), cursorPos.Y - ImGui.GetScrollY());
		float frameHeightWithSpacing = ImGui.GetFrameHeightWithSpacing();
		Vector2 p_max = vector + new Vector2(x, frameHeightWithSpacing);
		windowDrawList.AddRectFilled(vector, p_max, ImGui.GetColorU32(GlobalColors.text));
	}

	private static GetEnumObject CreateEnumObjectGetter(Type enumType)
	{
		return typeof(EnumDrawer).GetMethod("GetEnumObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(enumType).CreateDelegate<GetEnumObject>();
	}

	private static object GetEnumObject<TMember>(Entity entity, MemberPath memberPath, out Exception exception)
	{
		EntityUtils.GetEntityComponentMember<TMember>(entity, memberPath, out TMember value, out exception);
		return value;
	}
}
