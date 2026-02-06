namespace Monod.InputModule;

/// <summary>
/// Information about a single player, playing the game.
/// </summary>
public sealed class Player
{
    /// <summary>
    /// Index of the gamepad this player uses.
    /// </summary>
    public int GamepadIndex = -1;

    /// <summary>
    /// Whether this player uses keyboard/mouse devices, and should be considered using inputs from those devices.
    /// </summary>
    public bool UsesKeyboard = true;

    /// <summary>
    /// Whether last device used by this player was gamepad or keyboard/mouse. Used to determine which icons to show in tutorial/hints.
    /// </summary>
    public bool LastUsedDeviceIsGamepad = false;

    /// <summary>
    /// Input settings this player uses.
    /// </summary>
    private InputSettings? _settings = null;

    /// <summary>
    /// Input settings this player uses, or <see cref="Input.GlobalSettings"/> as fallback.
    /// </summary>
    public InputSettings Settings => _settings ?? Input.GlobalSettings;

    private InputMap? _map = null;
    public InputMap Map => _map ?? Input.DefaultMap;
}
