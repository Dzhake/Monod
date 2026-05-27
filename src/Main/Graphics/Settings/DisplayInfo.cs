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

    public string FancyName => $"{Name} ({Width}x{Height} @{RefreshRate:0}Hz)";

    public DisplayInfo(uint id, string name, int width, int height, float refreshRate, SDL.SDL_Rect bounds)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        RefreshRate = refreshRate;
        Bounds = bounds;
    }
}
