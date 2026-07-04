using System;
using System.Collections.Generic;
using System.Text;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class ExceptionUtils
{
	private static readonly StringBuilder ErrorBuilder = new StringBuilder();

	private static readonly Dictionary<Type, string> ExceptionCodes = new Dictionary<Type, string>();

	internal static StringBuilder GetExceptionDetails(Exception exception, MemberPath member, Entity entity)
	{
		StringBuilder errorBuilder = ErrorBuilder;
		string exceptionCode = GetExceptionCode(exception);
		errorBuilder.Clear();
		errorBuilder.Append(exceptionCode);
		errorBuilder.Append("   get => ");
		errorBuilder.Append(member.declaringType.Name);
		errorBuilder.Append(' ');
		errorBuilder.Append(member.path);
		errorBuilder.Append("   entity: ");
		errorBuilder.Append(entity.Id);
		errorBuilder.AppendLine();
		errorBuilder.AppendLine();
		errorBuilder.Append(exception.GetType().Name);
		errorBuilder.Append(": ");
		errorBuilder.Append(exception.Message);
		string stackTrace = exception.StackTrace;
		if (stackTrace != null)
		{
			int length = stackTrace.Length;
			length = stackTrace.LastIndexOf("   at ", length, StringComparison.InvariantCulture);
			if (length != -1)
			{
				length = stackTrace.LastIndexOf("   at ", length, StringComparison.InvariantCulture);
			}
			if (length > 0)
			{
				errorBuilder.AppendLine();
				ReadOnlySpan<char> readOnlySpan = stackTrace.AsSpan().Slice(0, length);
				if (readOnlySpan.EndsWith("\r\n"))
				{
					readOnlySpan = readOnlySpan.Slice(0, length - 2);
				}
				else if (readOnlySpan.EndsWith("\n"))
				{
					readOnlySpan = readOnlySpan.Slice(0, length - 1);
				}
				errorBuilder.Append(readOnlySpan);
			}
		}
		return errorBuilder;
	}

	internal static string GetExceptionCode(Exception exception)
	{
		Type type = exception.GetType();
		if (ExceptionCodes.TryGetValue(type, out string value))
		{
			return value;
		}
		StringBuilder errorBuilder = ErrorBuilder;
		errorBuilder.Clear();
		errorBuilder.Append('#');
		string name = type.Name;
		foreach (char c in name)
		{
			if (char.IsUpper(c))
			{
				errorBuilder.Append(c);
			}
		}
		value = errorBuilder.ToString();
		ExceptionCodes.Add(type, value);
		return value;
	}
}
