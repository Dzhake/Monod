namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is released.
/// </summary>
public class ReleasedAction(Key keybind) : KeyBasedAction(keybind)
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Released(Keybind, playerIndex);

    /// <summary>
    /// Name of this input action in serialization.
    /// </summary>
    public static readonly string NAME = "Released";

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{NAME}({Keybind})";
    }
}
