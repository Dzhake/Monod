using Monod.InputModule.InputActions;
using System.Text.Json.Serialization;

namespace Monod.InputModule;

/// <summary>
/// Manages multiple <see cref="InputActions"/>, providing wrapper methods general for all actions.
/// </summary>
[JsonConverter(typeof(InputMapConverter))]
public sealed class InputMap
{
    /// <summary>
    /// Dictionary, with the key being a value from <see cref="Input.ActionNames"/>, and the key being input action coresspoding that global action's index.
    /// </summary>
    public Dictionary<int, InputAction> Actions = new();

    /// <summary>
    /// Value of the action. Usually 0 or 1, but might be somewhere in between for specific actions.
    /// </summary>
    /// <param name="actionIndex">Index of the action in <see cref="Input.ActionNames"/>.</param>
    /// <param name="playerIndex">Index of the player for whom to get value. Affects how inputs are checked.</param>
    /// <returns>Value of the action. Usually 0 or 1, but might be somewhere in between for specific actions.</returns>
    public float GetValue(int actionIndex, int playerIndex) => Actions[actionIndex].GetValue(playerIndex);

    /// <summary>
    /// Whether action is considered active/non-zero.
    /// </summary>
    /// /// <param name="actionIndex">Index of the action in <see cref="Input.ActionNames"/>.</param>
    /// <param name="playerIndex">Index of the player for whom to check. Affects how inputs are checked.</param>
    /// <returns>Whether action is considered active/non-zero.</returns>
    public bool IsActive(int actionIndex, int playerIndex) => Actions[actionIndex].IsActive(playerIndex);

    /// <summary>
    /// Block the key used for the action, to prevent it from triggering again this frame.
    /// </summary>
    /// /// <param name="actionIndex">Index of the action in <see cref="Input.ActionNames"/>.</param>
    /// <param name="playerIndex">Index of the player for whom to block. Affects how inputs are blocked.</param>
    public void Block(int actionIndex, int playerIndex) => Actions[actionIndex].Block(playerIndex);

    /// <summary>
    /// Run <see cref="Block"/> if the <see cref="IsActive"/> is <see langword="true"/>, and return whether the action is active.
    /// </summary>
    /// /// <param name="actionIndex">Index of the action in <see cref="Input.ActionNames"/>.</param>
    /// <param name="playerIndex">Index of the player for whom to check/block. Affects how inputs are checked/blocked.</param>
    /// <returns>Whether the action is active.</returns>
    public bool BlockIfActive(int actionIndex, int playerIndex) => Actions[actionIndex].BlockIfActive(playerIndex);
}
