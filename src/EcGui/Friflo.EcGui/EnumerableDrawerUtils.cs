using System;

namespace Friflo.EcGui;

internal static class EnumerableDrawerUtils
{
	internal static string[] GetSortFields(Type enumerableType)
	{
		if (!(enumerableType.GetProperty("Count", typeof(int)) != null))
		{
			return new string[1] { "" };
		}
		return new string[1] { "Count" };
	}
}
