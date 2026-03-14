using Microsoft.Xna.Framework.Input;

namespace Monod.InputModule;

/// <summary>
/// State of the input at some frame.
/// </summary>
public class InputState
{
    /// <summary>
    /// State of the <see cref="Microsoft.Xna.Framework.Input.Keyboard"/>.
    /// </summary>
    public MonodKeyboardState Keyboard;

    /// <summary>
    /// State of the <see cref="Microsoft.Xna.Framework.Input.Mouse"/>
    /// </summary>
    public MouseState Mouse;

    /// <summary>
    /// State of each <see cref="GamePad"/>.
    /// </summary>
    public GamePadState[] Gamepads;

    /// <summary>
    /// Difference of (vertical) mouse wheel's position of this and previous frame.
    /// </summary>
    public float MouseWheelDiff;

    /// <summary>
    /// Difference of horizontal mouse wheel's position of this and previous frame.
    /// </summary>
    public float HorizontalMouseWheelDiff;

    /// <summary>
    /// Create a new instance of the <see cref="InputState"/> with the specified parameters.
    /// </summary>
    /// <param name="prevState">State of the input during the previous frame.</param>
    /// <param name="keyboard">State of the keyboard.</param>
    /// <param name="mouse">State of the mouse.</param>
    /// <param name="gamepads">State of the gamepads.</param>
    /// <param name="mouseWheelDiff">Movement of mouse wheel since last frame (vertically).</param>
    /// <param name="horizontalMouseWheelDiff">Movement of mouse wheel since last (horizontally).</param>
    public InputState(InputState prevState, KeyboardState keyboard, MouseState mouse, GamePadState[] gamepads, float mouseWheelDiff, float horizontalMouseWheelDiff)
    {
        Keyboard = new(keyboard, prevState.Keyboard);
        Mouse = mouse;
        Gamepads = gamepads;
        MouseWheelDiff = mouseWheelDiff;
        HorizontalMouseWheelDiff = horizontalMouseWheelDiff;
    }

    /// <summary>
    /// Create a new "empty" instance of the <see cref="InputState"/>, with all keys being up.
    /// </summary>
    public InputState()
    {
        Keyboard = new();
        Mouse = new();
        Gamepads = [];
        MouseWheelDiff = 0;
        HorizontalMouseWheelDiff = 0;
    }

    /// <summary>
    /// Check whether <see cref="Key.LeftControl"/> or <see cref="Key.RightControl"/> is down in this state.
    /// </summary>
    public bool Ctrl => Keyboard.IsKeyDown(Key.LeftControl) || Keyboard.IsKeyDown(Key.RightControl);

    /// <summary>
    /// Check whether <see cref="Key.LeftShift"/> or <see cref="Key.RightShift"/> is down in this state.
    /// </summary>
    public bool Shift => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

    /// <summary>
    /// Check whether <see cref="Key.LeftAlt"/> or <see cref="Key.RightAlt"/> is down in this state.
    /// </summary>
    public bool Alt => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

    public KeyModifiers GetActiveModifiers()
    {
        KeyModifiers modifiers = KeyModifiers.None;
        if (Ctrl) modifiers |= KeyModifiers.Ctrl;
        if (Shift) modifiers |= KeyModifiers.Shift;
        if (Alt) modifiers |= KeyModifiers.Alt;
        return modifiers;
    }
}
