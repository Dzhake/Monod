namespace Monod.InputModule.InputActions;

/// <summary>
/// Input action that could not be parsed from the given <see cref="Text"/>.
/// </summary>
public class InvalidInputAction : InputAction
{
    /// <summary>
    /// Text that could not be parsed as input action.
    /// </summary>
    public readonly string Text;

    /// <summary>
    /// Create a new instance of the <see cref="InvalidInputAction"/> with the specified <see cref="Text"/>.
    /// </summary>
    /// <param name="text">Text that could not be parsed as input action.</param>
    public InvalidInputAction(string text)
    {
        Text = text;
    }

    ///<inheritdoc/>
    public override void Block(int playerIndex = 0) { }

    ///<inheritdoc/>
    public override bool IsActive(int playerIndex = 0) => false;

    ///<inheritdoc/>
    public override string ToString()
    {
        return Text;
    }
}
