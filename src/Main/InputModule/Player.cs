namespace Monod.InputModule;

/// <summary>
/// Information about a single player, playing the game.
/// </summary>
public sealed class Player
{
    /// <summary>
    /// Index of the gamepad this player uses, or -1 if this player only uses keyboard.
    /// </summary>
    public int GamepadIndex = -1;

    /// <summary>
    /// Whether this player uses keyboard/mouse devices, and should be considered using inputs from those devices.
    /// </summary>
    public bool UsesKeyboard = true;

    /// <summary>
    /// Whether last device used by this player was gamepad or keyboard/mouse. Used to determine which icons to show in tutorial/hints.
    /// </summary>
    public bool LastUsedDeviceIsGamepad;

    /// <summary>
    /// Input settings this player uses.
    /// </summary>
    private InputSettings? _settings;

    /// <summary>
    /// Input settings this player uses, or <see cref="Input.GlobalSettings"/> as fallback.
    /// </summary>
    public InputSettings Settings => _settings ?? Input.GlobalSettings;

    private KeyMap? _map;
    public KeyMap Map
    {
        get
        {
            if (_map is null)
                _map = Input.DefaultMap.Clone();
            return _map;
        }

        set
        {
            _map = value;
        }
    }

    public void Update(InputState state, int playerIndex)
    {
        Map.Update(state, playerIndex);
    }
}
