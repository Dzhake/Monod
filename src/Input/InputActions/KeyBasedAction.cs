namespace Monod.InputModule.InputActions;

/// <summary>
/// Input actions that uses a <see cref="Keybind"/> as a base.
/// </summary>
public abstract class KeyBasedAction : InputAction
{
    /// <summary>
    /// Keybind this input action uses.
    /// </summary>
    public Key Keybind;

    /// <summary>
    /// Block <see cref="Keybind"/> for the given player.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to block. Affects how inputs are blocked.</param>
    public override void Block(int playerIndex = 0) => Input.Block(Keybind, playerIndex);

    /// <summary>
    /// Get value of this input action for the given player.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to get value. Affects how inputs are checked.</param>
    /// <returns>Value of this input action for the given player.</returns>
    public override float GetValue(int playerIndex) => Input.GetValue(Keybind, playerIndex);

    public KeyBasedAction(Key keybind)
    {
        Keybind = keybind;
    }
}
