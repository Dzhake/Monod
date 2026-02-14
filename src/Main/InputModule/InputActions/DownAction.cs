namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is down.
/// </summary>
public class DownAction(Key keybind) : KeyBasedAction(keybind)
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Down(Keybind, playerIndex);

    /// <summary>
    /// Name of this input action in serialization.
    /// </summary>
    public static readonly string NAME = "Down";

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{NAME}({Keybind})";
    }
}
