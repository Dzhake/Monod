using Microsoft.Xna.Framework;
using SDL3;

namespace Monod.Graphics;

/// <summary>
/// Modes of the window, that affect it's possible position, size and <see cref="GraphicsDeviceManager"/> values.
/// </summary>
public enum WindowMode
{
    /// <summary>
    /// Window size is set to screen size, and device.IsFullscreen is set to <see langword="true"/>.
    /// </summary>
    Fullscreen,

    /// <summary>
    /// Window size is any, window position is any.
    /// </summary>
    Windowed,

    /// <summary>
    /// Window size is set to screen size, and window position is set to <see cref="Point.Zero"/>.
    /// </summary>
    Borderless,

    /// <summary>
    /// <see cref="SDL.SDL_MaximizeWindow"/> is used.
    /// </summary>
    Maximized,
}
