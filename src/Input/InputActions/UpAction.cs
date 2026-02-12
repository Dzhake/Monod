namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is up.
/// </summary>
public class UpAction(Key keybind) : KeyBasedAction(keybind)
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Up(Keybind, playerIndex);

    /// <summary>
    /// Name of this input action in serialization.
    /// </summary>
    public static readonly string NAME = "Up";

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{NAME}({Keybind})";
    }
}
