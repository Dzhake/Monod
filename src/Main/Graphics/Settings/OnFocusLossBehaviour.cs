namespace Monod.Graphics.Settings;


/// <summary>
/// Represents types of behavior how game should act while not active.
/// </summary>
public enum OnFocusLossBehaviour
{
    /// <summary>
    /// Game continues running like it's active.
    /// </summary>
    Continue,

    /// <summary>
    /// Game will not update, but once it's active it'll run like all that time it was active, by storing deltaTime while not active, and once active using it all for first frame.
    /// </summary>
    Eco,

    /// <summary>
    /// Game will not update, and once it's active it'll continue like normal.
    /// </summary>
    TemporaryStop,

    /// <summary>
    /// Game will not update, and once it's active it will be paused.
    /// </summary>
    FullStop,
}
