using System;

namespace Friflo.EcGui;

internal sealed class ColumnFilterException : Exception
{
	internal ColumnFilterException(string message)
		: base(message)
	{
	}
}
