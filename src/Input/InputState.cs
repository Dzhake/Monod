using Microsoft.Xna.Framework.Input;

namespace Monod.InputModule;

/// <summary>
/// State of the input at some frame.
/// </summary>
/// <param name="keyboard">State of the keyboard.</param>
/// <param name="mouse">State of the mouse.</param>
/// <param name="gamepads">State of the gamepads.</param>
/// <param name="mouseWheelDiff">Movement of mouse wheel since last frame (vertically).</param>
/// <param name="horizontalMouseWheelDiff">Movement of mouse wheel since last (horizontally).</param>
public class InputState(KeyboardState keyboard, MouseState mouse, GamePadState[] gamepads, float mouseWheelDiff, float horizontalMouseWheelDiff)
{
    public KeyboardState Keyboard = keyboard;
    public MouseState Mouse = mouse;
    public GamePadState[] Gamepads = gamepads;
    public float MouseWheelDiff = mouseWheelDiff;
    public float HorizontalMouseWheelDiff = horizontalMouseWheelDiff;
}
