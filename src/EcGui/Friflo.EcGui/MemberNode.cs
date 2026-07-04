using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal readonly struct MemberNode
{
	internal readonly MemberPath path;

	internal readonly MemberNode[] children;

	internal readonly string name;

	public override string ToString()
	{
		return path.ToString();
	}

	internal MemberNode(MemberPath path, MemberNode[] children, string name)
	{
		this.path = path;
		this.children = children;
		this.name = name;
	}

	internal bool HasFilterMatches(string searchMember)
	{
		if (name.Contains(searchMember, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		MemberNode[] array = children;
		foreach (MemberNode memberNode in array)
		{
			if (memberNode.HasFilterMatches(searchMember))
			{
				return true;
			}
		}
		return false;
	}
}
