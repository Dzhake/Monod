using Microsoft.Xna.Framework;
using Monod.Graphics;

namespace Monod.TimeModule;

/// <summary>
/// Class for managing <see cref="DeltaTime"/> and <see cref="TotalTime"/>, and quick access to those.
/// </summary>
public static class Time
{
    /// <summary>
    /// GameTime passed to game's Update. For MLEM.
    /// </summary>
    public static GameTime gameTime;

    /// <summary>
    /// Unscaled time since last update.
    /// </summary>
    public static TimeSpan RawDeltaTimeSpan;

    /// <summary>
    /// Unscaled time since last update, in seconds.
    /// </summary>
    public static float RawDeltaTime;

    /// <summary>
    /// Time since last update, multiplied by <see cref="TimeScale"/>.
    /// </summary>
    public static TimeSpan DeltaTimeSpan;

    /// <summary>
    /// Time since last update, multiplied by timescale multipliers, in seconds.
    /// </summary>
    public static float DeltaTime;

    /// <summary>
    /// Time since program started.
    /// </summary>
    public static TimeSpan RawTotalDeltaTime;

    /// <summary>
    /// Time since program started, multiplied by timescale multipliers at moment those were active.
    /// </summary>
    public static TimeSpan TotalTime;

    /// <summary>
    /// Current time scale.
    /// </summary>
    public static float TimeScale = 1;

    /// <summary>
    /// Callback which is called when <see cref="RunTimeScaleCallbacks"/> is called. Subscribe to it if you want to change <see cref="TimeScale"/>.
    /// </summary>
    public static event Action TimeScaleCallback = delegate { };

    /// <summary>
    /// Was the game window active previous frame.
    /// </summary>
    private static bool wasActive = true;

    /// <summary>
    /// Updates everything related to time: deltaTime, totalTime, timeScale.
    /// </summary>
    /// <param name="gameTime"><see cref="GameTime"/> from your <see cref="Game.Update"/>.</param>
    /// <param name="isActive">Pass here <see cref="Game.IsActive"/>.</param>
    public static void Update(GameTime gameTime, bool isActive)
    {
        Time.gameTime = gameTime;
        if (GraphicsSettings.FocusLossBehaviour == GraphicsSettings.OnFocusLossBehaviour.Continue) isActive = true;

        if (wasActive || GraphicsSettings.FocusLossBehaviour > GraphicsSettings.OnFocusLossBehaviour.Eco)
            RawDeltaTimeSpan = gameTime.ElapsedGameTime;
        else
            RawDeltaTimeSpan += gameTime.ElapsedGameTime;
        wasActive = isActive;

        if (!isActive) return;
        RawTotalDeltaTime += RawDeltaTimeSpan;
        RunTimeScaleCallbacks();
        UpdateDeltaTime();
        TotalTime += DeltaTimeSpan;
    }

    /// <summary>
    /// Resets <see cref="TimeScale"/> to 1 and runs <see cref="TimeScaleCallback"/>.
    /// </summary>
    public static void RunTimeScaleCallbacks()
    {
        TimeScale = 1;
        TimeScaleCallback.Invoke();
    }

    /// <summary>
    /// Updates <see cref="DeltaTimeSpan"/> and <see cref="DeltaTime"/> to match <see cref="RawDeltaTimeSpan"/>.
    /// </summary>
    public static void UpdateDeltaTime()
    {
        DeltaTimeSpan = RawDeltaTimeSpan * TimeScale;
        DeltaTime = (float)DeltaTimeSpan.TotalSeconds;
        RawDeltaTime = (float)RawDeltaTimeSpan.TotalSeconds;
    }
}
