namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is down.
/// </summary>
public class DownAction : KeyBasedAction
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Down(Keybind, playerIndex);
}
