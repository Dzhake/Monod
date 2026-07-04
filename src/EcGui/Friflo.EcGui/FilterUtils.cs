using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class FilterUtils
{
	private static readonly Dictionary<Type, MemberInfo[]> MembersInfo = new Dictionary<Type, MemberInfo[]>();

	private static MemberExpression PropertyOrField(Expression expression, MemberInfo memberInfo)
	{
		if (memberInfo is FieldInfo field)
		{
			return Expression.Field(expression, field);
		}
		if (memberInfo is PropertyInfo property)
		{
			return Expression.Property(expression, property);
		}
		throw new InvalidOperationException("expect field or property");
	}

	internal static BinaryExpression CreateMatchExpression(MatchType matchType, ComponentType componentType, MemberInfo[] fieldPath, Type fieldType, object constant, out ParameterExpression arg)
	{
		arg = Expression.Parameter(componentType.Type.MakeByRefType(), "component");
		Expression expression = arg;
		foreach (MemberInfo memberInfo in fieldPath)
		{
			expression = PropertyOrField(expression, memberInfo);
		}
		if (fieldType.IsEnum)
		{
			Type underlyingType = Enum.GetUnderlyingType(fieldType);
			UnaryExpression unaryExpression = Expression.Convert(expression, underlyingType);
			if (matchType == MatchType.In)
			{
				return Get_IndexOf_InExpression(underlyingType, unaryExpression, constant);
			}
			UnaryExpression right = Expression.Convert(Expression.Constant(constant), underlyingType);
			return GetCompareMatchExpression(matchType, unaryExpression, right);
		}
		TypeCode typeCode = Type.GetTypeCode(fieldType);
		if ((uint)(typeCode - 4) <= 12u)
		{
			if (matchType == MatchType.In)
			{
				return Get_IndexOf_InExpression(fieldType, expression, constant);
			}
			return GetCompareMatchExpression(matchType, expression, Expression.Constant(constant));
		}
		switch (matchType)
		{
		case MatchType.In:
		{
			MethodInfo method3 = typeof(HashSet<>).MakeGenericType(fieldType).GetMethod("Contains");
			return Expression.Equal(Expression.Call(Expression.Constant(constant), method3, expression), Expression.Constant(true));
		}
		case MatchType.StartsWith:
		case MatchType.EndsWith:
		case MatchType.Contains:
			return GetStringCompareMatchExpression(matchType, expression, fieldType, constant);
		default:
		{
			Expression left;
			if (fieldType == typeof(string))
			{
				MethodInfo method = fieldType.GetMethod("CompareOrdinal", new Type[2] { fieldType, fieldType });
				left = Expression.Call(null, method, expression, Expression.Constant(constant));
			}
			else
			{
				MethodInfo method2 = fieldType.GetMethod("CompareTo", new Type[1] { fieldType });
				left = Expression.Call(expression, method2, Expression.Constant(constant));
			}
			return GetCompareMatchExpression(matchType, left, Expression.Constant(0));
		}
		}
	}

	private static BinaryExpression GetCompareMatchExpression(MatchType matchType, Expression left, Expression right)
	{
		return matchType switch
		{
			MatchType.LessThan => Expression.LessThan(left, right), 
			MatchType.LessThanOrEqual => Expression.LessThanOrEqual(left, right), 
			MatchType.GreaterThan => Expression.GreaterThan(left, right), 
			MatchType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right), 
			MatchType.Equal => Expression.Equal(left, right), 
			MatchType.NotEqual => Expression.NotEqual(left, right), 
			_ => throw new InvalidOperationException(), 
		};
	}

	private static BinaryExpression GetStringCompareMatchExpression(MatchType matchType, Expression fieldExpr, Type fieldType, object constant)
	{
		return Expression.Equal(Expression.Call(fieldExpr, matchType switch
		{
			MatchType.StartsWith => fieldType.GetMethod("StartsWith", new Type[2]
			{
				fieldType,
				typeof(StringComparison)
			}), 
			MatchType.EndsWith => fieldType.GetMethod("EndsWith", new Type[2]
			{
				fieldType,
				typeof(StringComparison)
			}), 
			MatchType.Contains => fieldType.GetMethod("Contains", new Type[2]
			{
				fieldType,
				typeof(StringComparison)
			}), 
			_ => throw new InvalidOperationException(), 
		}, Expression.Constant(constant), Expression.Constant(StringComparison.OrdinalIgnoreCase)), Expression.Constant(true));
	}

	private static BinaryExpression Get_IndexOf_InExpression(Type memberType, Expression fieldExpr, object filterElements)
	{
		MethodInfo method = GenericMethods.IndexOf.MakeGenericMethod(memberType);
		return Expression.GreaterThanOrEqual(Expression.Call(null, method, Expression.Constant(filterElements), fieldExpr), Expression.Constant(0));
	}

	private static bool Contains(string filterText, string op, out string path, out string value)
	{
		int num = filterText.IndexOf(op, StringComparison.Ordinal);
		if (num != -1)
		{
			int num2 = num + op.Length;
			value = filterText.Substring(num2, filterText.Length - num2);
			string input = filterText.Substring(0, num);
			path = Regex.Replace(input, "\\s+", "");
			return true;
		}
		value = "";
		path = "";
		return false;
	}

	internal static MatchType GetMatchType(Type type, string filterText, out string path, out string value, out string? error)
	{
		string text = filterText.Trim();
		error = null;
		if (Contains(text, "<=", out path, out value))
		{
			return MatchType.LessThanOrEqual;
		}
		if (Contains(text, "<", out path, out value))
		{
			return MatchType.LessThan;
		}
		if (Contains(text, ">=", out path, out value))
		{
			return MatchType.GreaterThanOrEqual;
		}
		if (Contains(text, ">", out path, out value))
		{
			return MatchType.GreaterThan;
		}
		if (Contains(text, "!=", out path, out value))
		{
			return MatchType.NotEqual;
		}
		if (Contains(text, "=", out path, out value))
		{
			return MatchType.Equal;
		}
		bool flag = text.StartsWith('*');
		bool flag2 = text.EndsWith('*');
		if ((flag || flag2) && type != typeof(string))
		{
			error = "* filter works only for string types";
			value = "";
			return MatchType.Undefined;
		}
		if (flag)
		{
			if (flag2)
			{
				if (text.Length > 1)
				{
					value = text.Substring(1, text.Length - 2);
				}
				else
				{
					value = "";
				}
				return MatchType.Contains;
			}
			value = text.Substring(1);
			return MatchType.EndsWith;
		}
		if (flag2)
		{
			value = text.Substring(0, text.Length - 1);
			return MatchType.StartsWith;
		}
		value = text;
		if (text.Contains(','))
		{
			return MatchType.In;
		}
		return MatchType.Equal;
	}

	internal static bool GetConstant(MemberPath? basePath, string path, Type type, string stringValue, MatchType matchType, out object? constant, out string? error)
	{
		string text = ((path == "") ? "" : ("." + path));
		string value = ((basePath == null) ? path : (basePath.declaringType.Name + " " + basePath.path + text));
		if (matchType == MatchType.In)
		{
			string[] array = stringValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
			object[] array2 = new object[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (!TypeUtils.ParseValue(type, array[i], out object result))
				{
					constant = null;
					error = $"invalid value '{array[i]}'. Expect - {value} : {TypeUtils.GetTypeName(type)}";
					return false;
				}
				array2[i] = result;
			}
			error = null;
			object[] array3;
			if (type == typeof(string))
			{
				Type type2 = typeof(HashSet<>).MakeGenericType(type);
				object obj = Activator.CreateInstance(type2);
				MethodInfo method = type2.GetMethod("Add");
				array3 = array2;
				foreach (object obj2 in array3)
				{
					method.Invoke(obj, new object[1] { obj2 });
				}
				constant = obj;
				return true;
			}
			Array array4 = Array.CreateInstance(type, array.Length);
			int num = 0;
			array3 = array2;
			foreach (object value2 in array3)
			{
				array4.SetValue(value2, num++);
			}
			constant = array4;
			return true;
		}
		if (TypeUtils.ParseValue(type, stringValue, out constant))
		{
			error = null;
			return true;
		}
		error = $"invalid value '{stringValue}'. Expect - {value} : {TypeUtils.GetTypeName(type)}";
		return false;
	}

	internal static MemberInfo[] GetMemberInfos(Type type)
	{
		if (MembersInfo.TryGetValue(type, out MemberInfo[] value))
		{
			return value;
		}
		List<MemberInfo> list = new List<MemberInfo>();
		MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (MemberInfo memberInfo in members)
		{
			if (memberInfo is FieldInfo)
			{
				if (!MemberUtils.IsAutoGeneratedBackingField(memberInfo.CustomAttributes))
				{
					list.Add(memberInfo);
				}
			}
			else if (memberInfo is PropertyInfo { CanRead: not false } propertyInfo && propertyInfo.GetIndexParameters().Length == 0)
			{
				list.Add(memberInfo);
			}
		}
		value = list.ToArray();
		MembersInfo.Add(type, value);
		return value;
	}
}
