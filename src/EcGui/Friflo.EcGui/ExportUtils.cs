using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class ExportUtils
{
	internal static void AppendMember(MemberFormat format, Entity entity, in MemberDrawer memberDrawer)
	{
		MemberPath member = memberDrawer.member;
		if (memberDrawer.typeDrawer is EnumDrawer enumDrawer)
		{
			Exception exception;
			object value = enumDrawer.getObject(entity, member, out exception);
			format.Append(value, exception);
			return;
		}
		switch (Type.GetTypeCode(member.memberType))
		{
		case TypeCode.Boolean:
		{
			EntityUtils.GetEntityComponentMember<bool>(entity, member, out var value7, out var exception7);
			format.Append(value7, exception7);
			return;
		}
		case TypeCode.SByte:
		{
			EntityUtils.GetEntityComponentMember<sbyte>(entity, member, out var value6, out var exception6);
			format.Append(value6, exception6);
			return;
		}
		case TypeCode.Int16:
		{
			EntityUtils.GetEntityComponentMember<short>(entity, member, out var value3, out var exception3);
			format.Append(value3, exception3);
			return;
		}
		case TypeCode.Int32:
		{
			EntityUtils.GetEntityComponentMember<int>(entity, member, out var value2, out var exception2);
			format.Append(value2, exception2);
			return;
		}
		case TypeCode.Int64:
		{
			EntityUtils.GetEntityComponentMember<long>(entity, member, out var value4, out var exception4);
			format.Append(value4, exception4);
			return;
		}
		case TypeCode.Byte:
		{
			EntityUtils.GetEntityComponentMember<byte>(entity, member, out var value5, out var exception5);
			format.Append(value5, exception5);
			return;
		}
		case TypeCode.UInt16:
		{
			EntityUtils.GetEntityComponentMember<ushort>(entity, member, out var value16, out var exception16);
			format.Append(value16, exception16);
			return;
		}
		case TypeCode.UInt32:
		{
			EntityUtils.GetEntityComponentMember<uint>(entity, member, out var value15, out var exception15);
			format.Append(value15, exception15);
			return;
		}
		case TypeCode.UInt64:
		{
			EntityUtils.GetEntityComponentMember<ulong>(entity, member, out var value14, out var exception14);
			format.Append(value14, exception14);
			return;
		}
		case TypeCode.Single:
		{
			EntityUtils.GetEntityComponentMember<float>(entity, member, out var value13, out var exception13);
			format.Append(value13, exception13);
			return;
		}
		case TypeCode.Double:
		{
			EntityUtils.GetEntityComponentMember<double>(entity, member, out var value12, out var exception12);
			format.Append(value12, exception12);
			return;
		}
		case TypeCode.Decimal:
		{
			EntityUtils.GetEntityComponentMember<decimal>(entity, member, out var value11, out var exception11);
			format.Append(value11, exception11);
			return;
		}
		case TypeCode.String:
		{
			EntityUtils.GetEntityComponentMember<string>(entity, member, out var value10, out var exception10);
			format.Append(value10, exception10);
			return;
		}
		case TypeCode.Char:
		{
			EntityUtils.GetEntityComponentMember<char>(entity, member, out var value9, out var exception9);
			format.Append(value9, exception9);
			return;
		}
		case TypeCode.DateTime:
		{
			EntityUtils.GetEntityComponentMember<DateTime>(entity, member, out var value8, out var exception8);
			format.Append(value8, exception8);
			return;
		}
		}
		TypeDrawer typeDrawer = memberDrawer.typeDrawer;
		if ((typeDrawer is UnsupportedTypeDrawer || typeDrawer is IObjectDrawer) ? true : false)
		{
			format.Append("---", null);
		}
		else
		{
			typeDrawer.Format(format);
		}
	}
}
