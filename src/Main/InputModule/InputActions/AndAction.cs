
namespace Monod.InputModule.InputActions;

/// <summary>
/// Action that is active when all of the <see cref="Actions"/> is active. Can't have non-integer value.
/// </summary>
public class AndAction : InputAction
{
    /// <summary>
    /// Actions that are checked.
    /// </summary>
    public InputAction[] Actions;

    /// <summary>
    /// Create a new instance of the <see cref="AndAction"/> with the given <see cref="Actions"/>.
    /// </summary>
    /// <param name="actions">Actions that are checked.</param>
    public AndAction(InputAction[] actions)
    {
        ArgumentNullException.ThrowIfNull(actions, nameof(actions));
        Actions = actions;
    }

    ///<inheritdoc/>
    public override void Block(int playerIndex = 0)
    {
        foreach (InputAction action in Actions) action.Block();
    }

    /// <inheritdoc/>
    public override bool IsActive(int playerIndex = 0)
    {
        foreach (InputAction action in Actions) if (!action.IsActive(playerIndex)) return false;
        return true;
    }

    /// <summary>
    /// Name of this input action in serialization.
    /// </summary>
    public static readonly string NAME = "And";

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{NAME}({string.Join(", ", Actions.Select(a => a.ToString()))})";
    }
}
