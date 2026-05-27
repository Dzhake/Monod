using SDL3;

namespace Monod.Graphics.Settings;

public class DisplayInfo
{
    public uint Id { get; init; }

    public string Name { get; init; } = "";

    public int Width { get; init; }
    public int Height { get; init; }

    public float RefreshRate { get; init; }

    public SDL.SDL_Rect Bounds { get; init; }

    public string FancyName;

    public DisplayInfo()
    {
        FancyName = $"{Name} ({Width}x{Height} @{RefreshRate:0}Hz)";
    }
}
