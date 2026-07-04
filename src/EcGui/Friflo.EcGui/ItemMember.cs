using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal sealed class ItemMember
{
	internal readonly MemberDrawer drawer;

	internal readonly string name;

	internal readonly Delegate getter;

	internal readonly Delegate setter;

	public override string ToString()
	{
		return drawer.member.ToString();
	}

	internal ItemMember(MemberDrawer drawer, Delegate getter, Delegate setter, string? customName)
	{
		this.drawer = drawer;
		this.getter = getter;
		this.setter = setter;
		if (customName != null)
		{
			name = customName;
			return;
		}
		MemberPath member = drawer.member;
		name = ((member.path == "") ? TypeUtils.GetTypeName(member.declaringType) : member.path);
	}
}
