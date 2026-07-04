namespace Friflo.EcGui;

internal struct TypeDrawerEntry
{
	internal readonly string? name;

	internal readonly TypeDrawer drawer;

	internal TypeDrawerEntry(TypeDrawer drawer)
	{
		name = null;
		this.drawer = drawer;
	}

	internal TypeDrawerEntry(string? name, TypeDrawer drawer)
	{
		this.name = name;
		this.drawer = drawer;
	}
}
