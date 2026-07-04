using System;
using System.Linq;
using System.Reflection;

namespace Friflo.EcGui;

internal static class GenericMethods
{
	internal static readonly MethodInfo IndexOf = Get_IndexOf_Method();

	private static MethodInfo Get_IndexOf_Method()
	{
		return typeof(Array).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(delegate(MethodInfo m)
		{
			if (!(m.Name == "IndexOf") || !m.IsGenericMethodDefinition)
			{
				return false;
			}
			ParameterInfo[] parameters = m.GetParameters();
			Type[] genericArguments = m.GetGenericArguments();
			return parameters.Length == 2 && genericArguments.Length == 1 && parameters[0].ParameterType == genericArguments[0].MakeArrayType() && parameters[1].ParameterType == genericArguments[0];
		});
	}
}
