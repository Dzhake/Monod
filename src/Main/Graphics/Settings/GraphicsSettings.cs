using Monod.Shared.Exceptions;
using Monod.Utils.Extensions;
using SDL3;

namespace Monod.Graphics.Settings;

/// <summary>
/// Represents graphics info which should be serialized in settings file and some methods to change those..
/// </summary>
public static class GraphicsSettings
{
    /// <summary>
    /// Whether the game should not update while the game is paused.
    /// </summary>
    public static OnFocusLossBehaviour FocusLossBehaviour = OnFocusLossBehaviour.TemporaryStop;

    public static WindowMode windowMode = WindowMode.Windowed;

    /// <summary>
    /// List of common 16:9 window sizes/resolutions.
    /// </summary>
    public static Point[] CommonResolutions16x9 =
    [
        new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3840,2160)
    ];

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

    public static bool ListenToEvents = true;

    public static DisplayInfo[] Displays = [];
    public static int SelectedDisplay = 0;
    public static DisplayInfo CurrentDisplay => Displays[SelectedDisplay];

    public static bool MouseLock;
    public static bool VSync;

    public static void Init()
    {
        SDL.SDL_SetWindowMinimumSize(Renderer.Window.Handle, 1280, 720);
        SDL.SDL_SetWindowAspectRatio(Renderer.Window.Handle, 16f / 9f, 16f / 9f);
        RefreshDisplays();
        ApplyWindowMode();
        ApplyMouseLock();
        ApplyVSync();
    }

    public static void ApplyMouseLock()
    {
        SDL.SDL_SetWindowMouseGrab(Renderer.Window.Handle, MouseLock);
    }

    public static void ApplyVSync()
    {
        Renderer.deviceManager.SynchronizeWithVerticalRetrace = VSync;
        Renderer.deviceManager.ApplyChanges();
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
            uint* displayIds = (uint*)ptr.ToPointer();

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
    /// Applies changes related to <see cref="windowMode"/> and calls <see cref="ApplyWindowSize"/>.
    /// </summary>
    public static void ApplyWindowMode()
    {
        bool fullscreen = windowMode == WindowMode.Fullscreen;
        DisplayInfo display = Displays[SelectedDisplay];
        GraphicsDeviceManager deviceManager = Renderer.deviceManager;

        nint window = Renderer.Window.Handle;
        switch (windowMode)
        {
            case WindowMode.Fullscreen:
                WindowSize = new(display.Width, display.Height);
                deviceManager.PreferredBackBufferWidth = WindowSize.X;
                deviceManager.PreferredBackBufferHeight = WindowSize.Y;
                break;
            case WindowMode.Windowed:
                //Fix window position not being (and, as the result, being set to 0,0) when swtching from fullscreen to windowed.
                if (deviceManager.IsFullScreen)
                {
                    deviceManager.IsFullScreen = false;
                    deviceManager.ApplyChanges();
                }

                SDL.SDL_RestoreWindow(window);
                WindowSize = new(1280, 720);
                break;
            case WindowMode.Borderless:
                SDL.SDL_RestoreWindow(window);
                WindowSize = new(display.Width, display.Height);
                break;
            case WindowMode.Maximized:
                SDL.SDL_MaximizeWindow(window);
                SDL.SDL_SyncWindow(window);
                SDL.SDL_GetWindowSize(window, out WindowSize.X, out WindowSize.Y);
                break;
        }

        //enabling IsBorderless breaks mouse position when window is not at 0,0 for some reason.
        Renderer.Window.IsBorderless = windowMode == WindowMode.Borderless;
        //setting IsFullScreen in borderless breaks window capturing (recording), maybe try to investigate?
        deviceManager.IsFullScreen = windowMode is WindowMode.Fullscreen;
        deviceManager.HardwareModeSwitch = fullscreen;
        ApplyWindowSize();
    }

    /// <summary>
    /// Applies changes related to <see cref="WindowSize"/>, calls <see cref="ApplyWindowPosition"/> and <see cref="GraphicsDeviceManager.ApplyChanges"/>.
    /// </summary>
    public static void ApplyWindowSize()
    {
        if (WindowSize.X <= 1280) WindowSize.X = 1280;
        float renderWidth = (float)Math.Floor(Math.Min(WindowSize.X, WindowSize.Y * MonodGame.AspectRatio));
        float renderHeight = renderWidth / MonodGame.AspectRatio;
        Renderer.RenderSize = new((int)renderWidth, (int)renderHeight);
        Renderer.RenderOffset = new((WindowSize.X - renderWidth) / 2, (WindowSize.Y - renderHeight) / 2);
        Renderer.deviceManager.PreferredBackBufferWidth = WindowSize.X;
        Renderer.deviceManager.PreferredBackBufferHeight = WindowSize.Y;
        ApplyWindowPosition();
        ListenToEvents = false;
        Renderer.deviceManager.ApplyChanges();
        ListenToEvents = true;
    }

    /// <summary>
    /// Applies changes related to <see cref="WindowPosition"/>.
    /// </summary>
    public static void ApplyWindowPosition()
    {
        Renderer.Window.Position = GetWindowPosition();
    }

    public static unsafe Point GetWindowPosition()
    {
        SDL.SDL_DisplayMode* displayMode = GetSelectDisplayMode();
        if (displayMode is null) return WindowPosition;
        if (windowMode == WindowMode.Windowed)
            return CenterWindow ? (new Point(displayMode->w, displayMode->h) - WindowSize).Divide(2) : WindowPosition;
        else
            return Point.Zero;
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

    public static bool IsWindowMaximized(nint window)
    {
        return SDL.SDL_GetWindowFlags(window).HasFlag(SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED);
    }
}