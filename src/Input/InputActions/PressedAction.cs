namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is pressed.
/// </summary>
public class PressedAction : KeyBasedAction
{
    ///<inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Pressed(Keybind, playerIndex);
}
