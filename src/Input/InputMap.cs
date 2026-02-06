using Monod.InputModule.InputActions;

namespace Monod.InputModule;

/// <summary>
/// Managers multiple <see cref="InputActions"/>, providing a
/// </summary>
public sealed class InputMap
{
    public Dictionary<int, InputAction> Actions = new();

    public float GetValue(int actionIndex, int playerIndex) => Actions[actionIndex].GetValue(playerIndex);
    public bool IsActive(int actionIndex, int playerIndex) => Actions[actionIndex].IsActive(playerIndex);
    public void Block(int actionIndex, int playerIndex) => Actions[actionIndex].Block(playerIndex);
    public bool BlockIfActive(int actionIndex, int playerIndex) => Actions[actionIndex].BlockIfActive(playerIndex);
}
