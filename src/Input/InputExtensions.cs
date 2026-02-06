using Microsoft.Xna.Framework.Input;

namespace Monod.InputModule;

public static class InputExtensions
{
    public static float ToInputValue(this bool value) => value ? 1f : 0f;
    public static float ToInputValue(this ButtonState value) => value == ButtonState.Pressed ? 1f : 0f;
}
