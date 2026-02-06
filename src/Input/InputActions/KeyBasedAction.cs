namespace Monod.InputModule.InputActions;

public abstract class KeyBasedAction : InputAction
{
    public Key Keybind;

    public override void Block(int playerIndex = 0) => Input.Block(Keybind, playerIndex);

    public override float GetValue(int playerIndex) => Input.GetValue(Keybind, playerIndex);
}
