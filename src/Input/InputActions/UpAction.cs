namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is up.
/// </summary>
public class UpAction : KeyBasedAction
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Up(Keybind, playerIndex);
}
