namespace Monod.InputModule.InputActions;

/// <summary>
/// An object that may be active or inactive and has a value based on certain inputs.
/// </summary>
public abstract class InputAction
{
    /// <summary>
    /// Whether action is considered active/non-zero.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to check. Affects how inputs are checked.</param>
    /// <returns>Whether action is considered active/non-zero.</returns>
    public abstract bool IsActive(int playerIndex = 0);

    /// <summary>
    /// Value of the action. Usually 0 or 1, but might be somewhere in between for specific actions.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to get value. Affects how inputs are checked.</param>
    /// <returns>Value of the action. Usually 0 or 1, but might be somewhere in between for specific actions.</returns>
    public virtual float GetValue(int playerIndex = 0) => IsActive(playerIndex) ? 1f : 0f;

    /// <summary>
    /// Block the key used for this actions, to prevent it from triggering again this frame.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to block. Affects how inputs are blocked.</param>
    public abstract void Block(int playerIndex = 0);

    /// <summary>
    /// Run <see cref="Block"/> if the <see cref="IsActive"/> is <see langword="true"/>, and return whether the action is active.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to check/block. Affects how inputs are checked/blocked.</param>
    /// <returns>Whether the action is active</returns>
    public bool BlockIfActive(int playerIndex = 0)
    {
        bool active = IsActive(playerIndex);
        if (active)
            Block(playerIndex);

        return active;
    }
}
