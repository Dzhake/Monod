using Microsoft.Xna.Framework;
using Monod.Shared.Exceptions;
using Monod.Utils.Extensions;
using SDL3;

namespace Monod.Graphics.Settings;

/// <summary>
/// Represents graphics info which should be serialized in settings file and some methods to change those..
/// </summary>
public static partial class GraphicsSettings
{
    /// <summary>
    /// Whether the game should not update while the game is paused.
    /// </summary>
    public static OnFocusLossBehaviour FocusLossBehaviour = OnFocusLossBehaviour.Eco;

    public static WindowMode windowMode = WindowMode.Windowed;

    /// <summary>
    /// List of common 16:9 window sizes/resolutions.
    /// </summary>
    public static Point[] CommonResolutions16x9 =
    [
        new (640, 360), new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3840,2160)
    ];

    /// <summary>
    /// List of common 4:3 window sizes/resolutions.
    /// </summary>
    public static Point[] CommonResolutions4x3 = [new(640, 480), new(800, 600), new(1600, 1200)];

    /// <summary>
    /// Game window's size in pixels.
    /// </summary>
    public static Point WindowSize = new(1280, 720);

    /// <summary>
    /// Position of the window relative to the screen, in pixels.
    /// </summary>
    public static Point WindowPosition;

    public static bool CenterWindow = true;

    /// <summary>
    /// Whether to save and load <see cref="WindowPosition"/>.
    /// </summary>
    public static bool KeepPosition = true;

    /// <summary>
    /// Whether to save and load <see cref="WindowSize"/>.
    /// </summary>
    public static bool KeepSize = true;

    public static DisplayInfo[] Displays = [];
    public static int SelectedDisplay = 0;
    public static DisplayInfo CurrentDisplay => Displays[SelectedDisplay];


    public static void Init()
    {
        RefreshDisplays();
        ApplyWindowModeChanges();
    }

    public static unsafe void RefreshDisplays()
    {
        nint ptr = SDL.SDL_GetDisplays(out int count);

        try
        {
            if (ptr == nint.Zero || count <= 0)
            {
                Guard.Exception(SDL.SDL_GetError());
                return;
            }

            Displays = new DisplayInfo[count];
            uint* displayIds = (uint*)(ptr.ToPointer());

            for (int i = 0; i < count; i++)
            {
                uint displayId = displayIds[i];

                var mode = SDL.SDL_GetCurrentDisplayMode(displayId);
                SDL.SDL_GetDisplayBounds(displayId, out var bounds);

                Displays[i] = new DisplayInfo(displayId, SDL.SDL_GetDisplayName(displayId), mode->w, mode->h, mode->refresh_rate, bounds);
            }
        }
        finally
        {
            SDL.SDL_free(ptr);
        }

        if (Displays.Length == 0) Guard.Exception("Could not find any displays.");
        if (SelectedDisplay >= Displays.Length) SelectedDisplay = 0;
    }

    /// <summary>
    /// Changes window behaviour to fullscreen or windowed.
    /// </summary>
    public static void ApplyWindowModeChanges()
    {
        bool fullscreen = windowMode == WindowMode.Fullscreen;
        DisplayInfo display = Displays[SelectedDisplay];
        GraphicsDeviceManager deviceManager = Renderer.deviceManager;

        switch (windowMode)
        {
            case WindowMode.Fullscreen:
                WindowSize = new(display.Width, display.Height);
                deviceManager.PreferredBackBufferWidth = WindowSize.X;
                deviceManager.PreferredBackBufferHeight = WindowSize.Y;
                break;
            case WindowMode.Windowed:
                SDL.SDL_RestoreWindow(Renderer.WindowHandle);
                WindowSize = new(1280, 720);
                break;
            case WindowMode.Borderless:
                SDL.SDL_RestoreWindow(Renderer.WindowHandle);
                WindowSize = new(display.Width, display.Height);
                WindowPosition = Point.Zero;
                break;
            case WindowMode.Maximized:
                SDL.SDL_MaximizeWindow(Renderer.WindowHandle);
                break;
        }

        deviceManager.IsFullScreen = fullscreen;
        deviceManager.HardwareModeSwitch = !fullscreen;
        ApplyWindowSizeChanges();
    }

    /// <summary>
    /// Changes PreferredBackBuffer sizes and centers game's window in windowed mode
    /// </summary>
    public static void ApplyWindowSizeChanges()
    {
        if (WindowSize.X <= 480) WindowSize.X = 480;
        if (WindowSize.Y <= 480) WindowSize.Y = 480;
        Renderer.deviceManager.PreferredBackBufferWidth = WindowSize.X;
        Renderer.deviceManager.PreferredBackBufferHeight = WindowSize.Y;
        ApplyWindowPositionChanges();
        Renderer.deviceManager.ApplyChanges();
    }

    public static unsafe void ApplyWindowPositionChanges()
    {
        SDL.SDL_DisplayMode* displayMode = GetSelectDisplayMode();
        if (displayMode is null) return;
        Renderer.Window.Position = CenterWindow ? (new Point(displayMode->w, displayMode->h) - WindowSize).Divide(2) : WindowPosition;
    }

    private static unsafe SDL.SDL_DisplayMode* GetSelectDisplayMode()
    {
        var displayMode = SDL.SDL_GetCurrentDisplayMode(CurrentDisplay.Id);
        if (displayMode is null)
        {
            string error = SDL.SDL_GetError();
            Guard.Exception($"SDL error in GraphicsSettings.ApplyWindowSizeChanges: {error}");
        }

        return displayMode;
    }
}