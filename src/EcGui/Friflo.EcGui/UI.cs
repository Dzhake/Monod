namespace Friflo.EcGui;

internal static class UI
{
	internal const string More = "...";

	internal static float scale;

	public const string Field = "##field";

	public static float Scl(float pixel)
	{
		return pixel * scale;
	}

	public static int SclToInt(float pixel)
	{
		return (int)(pixel * scale);
	}
}
