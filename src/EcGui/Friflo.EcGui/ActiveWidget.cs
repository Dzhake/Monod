namespace Friflo.EcGui;

internal static class ActiveWidget
{
	internal static InputIdentifier identifier;

	internal static bool IsActive(in DrawValue drawValue)
	{
		if (identifier.entity == drawValue.context.entity && identifier.memberPath == drawValue.memberDrawer.member)
		{
			return identifier.context == drawValue.context;
		}
		return false;
	}
}
