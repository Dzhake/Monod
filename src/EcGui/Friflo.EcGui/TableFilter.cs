using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class TableFilter
{
	internal static void Filter(EntityList entities, List<Column> columns)
	{
		foreach (Column column in columns)
		{
			if (column.HasFilter)
			{
				entities.Filter(column.GetFilterPredicate());
			}
		}
	}

	internal static FieldFilter[] CreateFieldFilters(Column column, out TermOperator termOperator, out string? error)
	{
		termOperator = TermOperator.And;
		string text = column.filterText.Trim();
		if (text.Trim() == "")
		{
			error = null;
			return Array.Empty<FieldFilter>();
		}
		if (text.EndsWith('&') || text.EndsWith('|'))
		{
			text = text.Substring(0, text.Length - 1);
		}
		string[] array = text.Split("&", StringSplitOptions.TrimEntries);
		string[] array2 = text.Split("|", StringSplitOptions.TrimEntries);
		if (array.Length == 1 && array2.Length == 1)
		{
			FieldFilter fieldFilter = CreateFieldFilter(column, text, out error);
			if (fieldFilter != null)
			{
				return new FieldFilter[1] { fieldFilter };
			}
			return Array.Empty<FieldFilter>();
		}
		if (array.Length > 1 && array2.Length > 1)
		{
			error = "cannot mix & and | operators to preserve simplicity";
			return Array.Empty<FieldFilter>();
		}
		List<FieldFilter> list = new List<FieldFilter>();
		string[] array3 = array;
		if (array2.Length > 1)
		{
			termOperator = TermOperator.Or;
			array3 = array2;
		}
		string[] array4 = array3;
		foreach (string filterText in array4)
		{
			FieldFilter fieldFilter2 = CreateFieldFilter(column, filterText, out error);
			if (error != null)
			{
				return Array.Empty<FieldFilter>();
			}
			if (fieldFilter2 != null)
			{
				list.Add(fieldFilter2);
			}
		}
		error = null;
		return list.ToArray();
	}

	private static FieldFilter? CreateFieldFilter(Column column, string filterText, out string? error)
	{
		if (!(column is FieldColumn fieldColumn))
		{
			if (!(column is TagColumn tagColumn))
			{
				if (column is IdColumn)
				{
					string path;
					string value;
					MatchType matchType = FilterUtils.GetMatchType(typeof(int), filterText, out path, out value, out error);
					if (error != null)
					{
						return null;
					}
					if (!FilterUtils.GetConstant(null, "Id", typeof(int), value, matchType, out object constant, out error))
					{
						if (value == "")
						{
							error = null;
							return null;
						}
						return null;
					}
					if (matchType == MatchType.In)
					{
						return new EntityIdFilter(filterText, matchType, -1, (int[])constant);
					}
					return new EntityIdFilter(filterText, matchType, (int)constant, null);
				}
				error = "column support no filter";
				return null;
			}
			bool hasTag;
			switch (filterText)
			{
			case "*":
			case "true":
				hasTag = true;
				break;
			case "!":
			case "false":
				hasTag = false;
				break;
			default:
				error = $"invalid value '{filterText}'. Expect - {tagColumn.tagType.Name} : true false * !";
				return null;
			}
			error = null;
			return new TagFilter(filterText, tagColumn.tagType, hasTag);
		}
		return CreateFilter(fieldColumn.memberDrawer.member, filterText, out error);
	}

	private static FieldFilter? CreateFilter(MemberPath member, string filterText, out string? error)
	{
		ComponentType componentType = member.componentType;
		if (filterText == "*")
		{
			error = null;
			return new ComponentExistFilter(filterText, componentType, exist: true);
		}
		if (filterText == "!")
		{
			error = null;
			return new ComponentExistFilter(filterText, componentType, exist: false);
		}
		try
		{
			MatchInfo matchInfo = new MatchInfo
			{
				component = componentType.Name,
				field = member.path,
				filter = filterText
			};
			string path;
			string value;
			MatchType matchType = FilterUtils.GetMatchType(member.memberType, filterText, out path, out value, out error);
			if (error != null)
			{
				return null;
			}
			MemberInfo[] pathArray;
			Type fieldType = GetFieldType(member, path, filterText, out pathArray, out error);
			if (fieldType == null)
			{
				return null;
			}
			if (!FilterUtils.GetConstant(member, path, fieldType, value, matchType, out object constant, out error))
			{
				if (value == "")
				{
					error = null;
					return null;
				}
				return null;
			}
			ParameterExpression arg;
			BinaryExpression binaryExpression = FilterUtils.CreateMatchExpression(matchType, member.componentType, pathArray, fieldType, constant, out arg);
			return (FieldFilter)typeof(FieldFilter<>).MakeGenericType(componentType.Type).GetMethod("CreateFilterLambda", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[5] { filterText, componentType, binaryExpression, arg, matchInfo });
		}
		catch (Exception ex)
		{
			error = ex.GetType().Name + " : " + ex.Message;
			return null;
		}
	}

	private static Type? GetFieldType(MemberPath memberPath, string path, string filterText, out MemberInfo[] pathArray, out string? error)
	{
		Type type = memberPath.componentType.Type;
		string[] array = ((path == "") ? memberPath.path : (memberPath.path + "." + path)).Split('.', StringSplitOptions.RemoveEmptyEntries);
		pathArray = new MemberInfo[array.Length];
		Type type2 = type;
		for (int i = 0; i < array.Length; i++)
		{
			string field = array[i];
			MemberInfo[] array2 = (from m in FilterUtils.GetMemberInfos(type2)
				where m.Name == field
				select m).ToArray();
			if (array2.Length == 0)
			{
				string[] comparableFields = GetComparableFields(type2);
				if (Array.IndexOf(comparableFields, filterText) >= 0)
				{
					error = null;
					return null;
				}
				string text = string.Join(',', comparableFields);
				error = "field '" + field + "' not found. Use field: " + text;
				return null;
			}
			MemberInfo memberInfo = array2[0];
			pathArray[i] = memberInfo;
			if (memberInfo is FieldInfo fieldInfo)
			{
				type2 = fieldInfo.FieldType;
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				type2 = propertyInfo.PropertyType;
			}
		}
		if (IsComparable(type2))
		{
			error = null;
			return type2;
		}
		string[] comparableFields2 = GetComparableFields(type2);
		if (Array.IndexOf(comparableFields2, filterText) >= 0)
		{
			error = null;
			return null;
		}
		string text2 = string.Join(',', comparableFields2);
		error = "cannot filter type: " + type2.Name + " - use field: " + text2;
		return null;
	}

	private static bool IsComparable(Type type)
	{
		TypeCode typeCode = Type.GetTypeCode(type);
		if ((uint)(typeCode - 3) <= 13u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	private static string[] GetComparableFields(Type type)
	{
		if (type == typeof(string))
		{
			return new string[1] { "Length" };
		}
		if (type == typeof(Entity))
		{
			return new string[3] { "Id", "Revision", "IsNull" };
		}
		return (from i in FilterUtils.GetMemberInfos(type).Where(delegate(MemberInfo member)
			{
				Type type2 = null;
				if (member is FieldInfo fieldInfo)
				{
					type2 = fieldInfo.FieldType;
				}
				else if (member is PropertyInfo propertyInfo)
				{
					type2 = propertyInfo.PropertyType;
				}
				return IsComparable(type2);
			})
			select i.Name).ToArray();
	}
}
