namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is held.
/// </summary>
public class HeldAction : KeyBasedAction
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Held(Keybind, playerIndex);
}
