namespace Monod.Utils.Extensions;

public static class ColorExtensions
{
    public static void AddRgb(this ref Color color, byte s)
    {
        color.R += s;
        color.G += s;
        color.B += s;
    }
}
