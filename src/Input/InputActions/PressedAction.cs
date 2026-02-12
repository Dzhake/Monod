namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is pressed.
/// </summary>
public class PressedAction(Key keybind) : KeyBasedAction(keybind)
{
    ///<inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Pressed(Keybind, playerIndex);

    /// <summary>
    /// Name of this input action in serialization.
    /// </summary>
    public static readonly string NAME = "Pressed";

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{NAME}({Keybind})";
    }
}
