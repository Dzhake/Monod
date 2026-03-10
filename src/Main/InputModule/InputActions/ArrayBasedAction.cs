

namespace Monod.InputModule.InputActions;

public abstract class ArrayBasedAction : InputAction
{
    /// <summary>
    /// Actions that are checked.
    /// </summary>
    public InputAction[] Actions;

    public abstract string Name { get; }

    public abstract string JoinName { get; }

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{Name}({string.Join(", ", Actions)})";
    }
}
