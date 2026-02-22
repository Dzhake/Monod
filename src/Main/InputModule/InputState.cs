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
    /// <summary>
    /// State of the <see cref="Microsoft.Xna.Framework.Input.Keyboard"/>.
    /// </summary>
    public KeyboardState Keyboard = keyboard;

    /// <summary>
    /// State of the <see cref="Microsoft.Xna.Framework.Input.Mouse"/>
    /// </summary>
    public MouseState Mouse = mouse;

    /// <summary>
    /// State of each <see cref="GamePad"/>.
    /// </summary>
    public GamePadState[] Gamepads = gamepads;

    /// <summary>
    /// Difference of (vertical) mouse wheel's position of this and previous frame.
    /// </summary>
    public float MouseWheelDiff = mouseWheelDiff;

    /// <summary>
    /// Difference of horizontal mouse wheel's position of this and previous frame.
    /// </summary>
    public float HorizontalMouseWheelDiff = horizontalMouseWheelDiff;

    /// <summary>
    /// List of keys blocked at this frame. Keys in this list should be ignored/have zero value when checked for player with index being playerIndex.
    /// </summary>
    public List<(int playerIndex, Key key)> BlockedKeys = new();
}
