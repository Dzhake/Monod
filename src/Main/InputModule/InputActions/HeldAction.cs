namespace Monod.InputModule.InputActions;

/// <summary>
/// Key based action, that triggers when the key is held.
/// </summary>
public class HeldAction(Key keybind) : KeyBasedAction(keybind)
{
    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => Input.Held(Keybind, playerIndex);

    /// <summary>
    /// Name of this input action in serialization.
    /// </summary>
    public static readonly string NAME = "Held";

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{NAME}({Keybind})";
    }
}
