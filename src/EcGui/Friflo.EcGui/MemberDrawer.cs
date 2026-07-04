using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal readonly struct MemberDrawer
{
	internal readonly MemberPath member;

	internal readonly string label;

	internal readonly ComponentType componentType;

	internal readonly TypeDrawer typeDrawer;

	internal readonly MemberWidget? widget;

	public override string ToString()
	{
		return member.ToString();
	}

	internal MemberDrawer(MemberPath member, TypeDrawer typeDrawer)
	{
		widget = null;
		this.member = member;
		label = this.member.name + "     ";
		this.typeDrawer = typeDrawer;
		componentType = member.componentType;
	}

	private MemberDrawer(MemberPath member, TypeDrawer typeDrawer, MemberWidget? widget)
	{
		this.member = member;
		label = this.member.name + "     ";
		this.typeDrawer = typeDrawer;
		componentType = member.componentType;
		this.widget = widget;
	}

	internal static MemberDrawer Create(MemberPath memberPath)
	{
		TypeDrawer typeDrawer = MemberUtils.GetTypeDrawer(memberPath);
		MemberWidget memberWidget = MemberWidget.GetMemberWidget(memberPath);
		return new MemberDrawer(memberPath, typeDrawer, memberWidget);
	}
}
