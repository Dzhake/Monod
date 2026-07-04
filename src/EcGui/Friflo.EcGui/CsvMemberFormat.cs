using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class CsvMemberFormat : MemberFormat
{
	private readonly StringBuilder stringBuilder = new StringBuilder();

	private int valueStartPos;

	internal static readonly MemberFormat Instance = new CsvMemberFormat();

	private static char[] _charBuffer = Array.Empty<char>();

	internal override string WriteAsString(Column[] columns, IReadOnlyList<Entity> entities)
	{
		stringBuilder.Clear();
		WriteHeader(columns);
		WriteEntities(columns, entities);
		return stringBuilder.ToString();
	}

	private void WriteHeader(Column[] columns)
	{
		StringBuilder stringBuilder = this.stringBuilder;
		stringBuilder.Append("Id,");
		foreach (Column column in columns)
		{
			if (column is TagColumn tagColumn)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder obj = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
				handler.AppendFormatted(tagColumn.tagType.Name);
				handler.AppendLiteral(",");
				obj.Append(ref handler);
			}
			else
			{
				if (!(column is FieldColumn fieldColumn))
				{
					continue;
				}
				MemberDrawer memberDrawer = fieldColumn.memberDrawer;
				MemberPath member = memberDrawer.member;
				string[] formatFields = memberDrawer.typeDrawer.FormatFields;
				if (formatFields != null)
				{
					string[] array = formatFields;
					foreach (string value in array)
					{
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder obj2 = stringBuilder2;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 3, stringBuilder2);
						handler.AppendFormatted(member.componentType.Name);
						handler.AppendLiteral(" ");
						handler.AppendFormatted(member.path);
						handler.AppendLiteral(".");
						handler.AppendFormatted(value);
						handler.AppendLiteral(",");
						obj2.Append(ref handler);
					}
				}
				else
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder obj3 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
					handler.AppendFormatted(member.componentType.Name);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(member.path);
					handler.AppendLiteral(",");
					obj3.Append(ref handler);
				}
			}
		}
		stringBuilder.Length--;
		stringBuilder.AppendLine();
	}

	private void WriteEntities(Column[] columns, IReadOnlyList<Entity> entities)
	{
		StringBuilder stringBuilder = this.stringBuilder;
		foreach (Entity entity2 in entities)
		{
			Entity entity = (currentEntity = entity2);
			stringBuilder.Append(entity.Id);
			stringBuilder.Append(',');
			foreach (Column column in columns)
			{
				if (column is TagColumn tagColumn)
				{
					bool flag = entity.Tags.HasAll(in tagColumn.tags);
					stringBuilder.Append(flag ? "true," : "false,");
				}
				else if (column is FieldColumn fieldColumn)
				{
					ComponentTypes componentTypes = entity.Archetype.ComponentTypes;
					MemberDrawer memberDrawer = fieldColumn.memberDrawer;
					if (componentTypes.Contains(memberDrawer.componentType))
					{
						valueStartPos = stringBuilder.Length;
						currentMember = memberDrawer.member;
						ExportUtils.AppendMember(this, entity, in memberDrawer);
					}
					else
					{
						string[]? formatFields = memberDrawer.typeDrawer.FormatFields;
						int repeatCount = ((formatFields == null) ? 1 : formatFields.Length);
						stringBuilder.Append(',', repeatCount);
					}
				}
			}
			stringBuilder.Length--;
			stringBuilder.AppendLine();
		}
	}

	public override void NotImplemented()
	{
		stringBuilder.Append("n/i");
		Sep();
	}

	public override void Append(sbyte value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(short value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(int value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(long value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(byte value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(ushort value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(uint value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(ulong value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(float value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
		}
		Sep();
	}

	public override void Append(double value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
		}
		Sep();
	}

	public override void Append(decimal value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
		}
		Sep();
	}

	public override void Append(char value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	public override void Append(bool value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value ? "true" : "false");
		}
		Sep();
	}

	public override void Append(string value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
			Escape(stringBuilder, valueStartPos);
		}
		Sep();
	}

	public override void Append(DateTime value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			char[] buffer = GetBuffer(32);
			value.TryFormat(buffer, out var charsWritten, "yyyy-MM-dd HH:mm:ss.FFFFFF");
			stringBuilder.Append(buffer, 0, charsWritten);
		}
		Sep();
	}

	public override void Append(object value, Exception? exception)
	{
		if (exception != null)
		{
			Error(exception);
		}
		else
		{
			stringBuilder.Append(value);
		}
		Sep();
	}

	private void Sep()
	{
		stringBuilder.Append(',');
		valueStartPos = stringBuilder.Length;
	}

	private void Error(Exception exception)
	{
		string exceptionCode = ExceptionUtils.GetExceptionCode(exception);
		stringBuilder.Append(exceptionCode);
	}

	private static void Escape(StringBuilder sb, int start)
	{
		int num = sb.Length - start;
		char[] buffer = GetBuffer(num);
		sb.CopyTo(start, buffer, 0, num);
		if (Array.IndexOf(buffer, ',', 0, num) == -1 && Array.IndexOf(buffer, '"', 0, num) == -1)
		{
			return;
		}
		sb.Length = start;
		sb.Append('"');
		for (int i = 0; i < num; i++)
		{
			char c = buffer[i];
			if (c == '"')
			{
				sb.Append('"');
			}
			sb.Append(c);
		}
		sb.Append('"');
	}

	private static char[] GetBuffer(int length)
	{
		char[] array = _charBuffer;
		if (array.Length < length)
		{
			array = (_charBuffer = new char[length + 100]);
		}
		return array;
	}
}
