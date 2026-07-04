using System.Collections.Generic;
using System.Reflection;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal abstract class MemberWidget
{
	internal static MemberWidget? GetMemberWidget(MemberPath memberPath)
	{
		MemberInfo memberInfo = memberPath.memberInfo;
		if ((object)memberInfo == null)
		{
			return null;
		}
		foreach (CustomAttributeData customAttribute in memberInfo.CustomAttributes)
		{
			if (customAttribute.AttributeType == typeof(UiDragAttribute))
			{
				IList<CustomAttributeTypedArgument> constructorArguments = customAttribute.ConstructorArguments;
				return new DragWidget
				{
					speed = (float)constructorArguments[0].Value,
					min = (float)constructorArguments[1].Value,
					max = (float)constructorArguments[2].Value,
					format = (string)constructorArguments[3].Value
				};
			}
		}
		return null;
	}
}
