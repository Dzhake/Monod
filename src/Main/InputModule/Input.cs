using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monod.LogModule;
using Monod.Shared.Enums;
using Monod.Shared.Extensions;
using Serilog;
using System.Diagnostics.Contracts;
using System.Text;

namespace Monod.InputModule;

/// <summary>
/// Helper class to manage everything related to input.
/// </summary>
public static class Input
{
    /// <summary>
    /// The lowest <see cref="Key"/> value that is an existing value, all values from this one to 0 are guarateed to exist, values above 0 are not.
    /// </summary>
    public static readonly Key MIN_EXISTING_KEY = Key.GamepadDPadDown;

    /// <summary>
    /// <see cref="ILogger"/> that should be used by the InputModule.
    /// </summary>
    public static readonly ILogger Logger = LogHelper.ForModule("Input");

    /// <summary>
    /// <see cref="StringBuilder"/> which contains "text input" keys pressed since last update, excluding <see cref="char.IsControl(char)"/> keys.
    /// </summary>
    public static StringBuilder? KeyString;

    /// <summary>
    /// The maximum number of game pads supported on this system.
    /// </summary>
    public static int MaximumGamePadCount = GamePad.MaximumGamePadCount;

    /// <summary>
    /// All players currently playing the game.
    /// </summary>
    public static Player[] Players = [new()];

    /// <summary>
    /// Default/fallback settings for <see cref="Players"/>.
    /// </summary>
    public static InputSettings GlobalSettings = new();

    /// <summary>
    /// Current state of the input devices.
    /// </summary>
    public static InputState CurState = new();

    /// <summary>
    /// State of the input devices on previous frame.
    /// </summary>
    public static InputState PrevState = new();

    /// <summary>
    /// Default/backup input map for all players. Should not be modified, and should be used for "restore to defaults".
    /// </summary>
    public static KeyMap DefaultMap = new();

    /// <summary>
    /// Enum of all possible actions, to be used with <see cref="KeyMap"/>, to have a way to globally define an action (for serialization/getting input action for the given player by name).
    /// </summary>
    public static NamedExtEnum ActionNames = new();

    public static HashSet<Key> KeyboardKeysPressed = new();
    public static HashSet<Key> KeyboardKeysReleased = new();
    public static HashSet<Key>?[] GamepadKeyUps;


    #region RequiredToCall
    /// <summary>
    /// Call in the <see cref="Game.Initialize"/>.
    /// </summary>
    /// <param name="game">Current game.</param>
    public static void Initialize(Game game)
    {
        KeyString = new StringBuilder();
        GameWindow win = game.Window;
        win.TextInput += OnTextInput;
        win.KeyDown += OnKeyDown;
        win.KeyUp += OnKeyUp;
    }

    private static void OnKeyUp(object? sender, InputKeyEventArgs e) => KeyboardKeysReleased.Add((Key)e.Key);

    private static void OnKeyDown(object? sender, InputKeyEventArgs e) => KeyboardKeysPressed.Add((Key)e.Key);

    /// <summary>
    /// Event subscriber called on the <see cref="GameWindow.TextInput"/>.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private static void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (char.IsControl(e.Character)) return;
        KeyString?.Append(e.Character);
    }


    /// <summary>
    /// Call in <see cref="Game"/>'s <see cref="Game.Update"/>, before anything that uses <see cref="Input"/> ".
    /// </summary>
    public static void Update()
    {
        PrevState = CurState;

        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();
        float mouseWheelDiff = mouse.ScrollWheelValue - PrevState.Mouse.ScrollWheelValue;
        float horizontalMouseWheelDiff = mouse.HorizontalScrollWheelValue - PrevState.Mouse.HorizontalScrollWheelValue;
        GamePadState[] gamepads = new GamePadState[MaximumGamePadCount];
        for (int i = 0; i < MaximumGamePadCount; i++)
        {
            gamepads[i] = GamePad.GetState(i);
        }

        CurState = new(PrevState, keyboard, mouse, gamepads, mouseWheelDiff, horizontalMouseWheelDiff);
        CurState.Keyboard.DownKeys.AddRange(KeyboardKeysPressed);
        CurState.Keyboard.ReleasedKeys.AddRange(KeyboardKeysReleased);


        //Update players and their last used device
        for (int i = 0; i < Players.Length; i++)
        {
            Player player = Players[i];
            player.Update(CurState, i);

            if (!player.UsesKeyboard)
            {
                player.LastUsedDeviceIsGamepad = true;
                continue;
            }
            if (keyboard.GetPressedKeys().Length != 0) player.LastUsedDeviceIsGamepad = false;
            if (player.GamepadIndex >= 0 && GamepadStateChanged(player.GamepadIndex)) player.LastUsedDeviceIsGamepad = true;
        }
    }



    /// <summary>
    /// Call in <see cref="Game.Update"/>, after everything that uses <see cref="Input"/>.
    /// </summary>
    public static void PostUpdate()
    {
        KeyString?.Clear();
        KeyboardKeysPressed.Clear();
        KeyboardKeysReleased.Clear();
    }
    #endregion

    #region ReadingInput
    public static Key FirstKeyReleased(int playerIndex)
    {
        if (Players[playerIndex].UsesKeyboard && KeyboardKeysReleased.Count > 0)
            return KeyboardKeysReleased.First();

        for (Key i = MIN_EXISTING_KEY; i < Key.None; i++)
            if (KeyReleased(i, playerIndex)) return i;

        return Key.None;
    }

    /// <summary>
    /// Get the first (random/any) key that was pressed this frame.
    /// </summary>
    /// <param name="playerIndex">Index of the player for whom to get the first key. Affects how input is checked.</param>
    /// <returns>First key that was pressed this frame.</returns>
    [Pure]
    public static Key FirstKeyPressed(int playerIndex)
    {
        if (Players[playerIndex].UsesKeyboard && KeyboardKeysPressed.Count > 0)
            return KeyboardKeysPressed.First();

        for (Key i = MIN_EXISTING_KEY; i < Key.None; i++)
            if (KeyPressed(i, playerIndex)) return i;

        return Key.None;
    }

    public static bool AnyKey(int playerIndex) => FirstKeyPressed(playerIndex) != Key.None;

    /// <summary>
    /// Check whether the <paramref name="key"/> is a key on a keyboard.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Whether the <paramref name="key"/> is a key on a keyboard.</returns>
    [Pure]
    public static bool IsKeyboardKey(Key key) => key >= 0;

    /// <summary>
    /// Get value of the <paramref name="key"/> in the <paramref name="state"/> for the <paramref name="playerIndex"/>.
    /// </summary>
    /// <param name="state">State, where to check the keys.</param>
    /// <param name="key">Key, whose value to get.</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns>Value of the <paramref name="key"/> in the <paramref name="state"/> for the <paramref name="playerIndex"/>.</returns>
    [Pure]
    public static float GetValue(InputState state, Key key, int playerIndex = 0)
    {
        if (!Players[playerIndex].UsesKeyboard && !IsGamepadKey(key)) return 0;
        if (IsGamepadKey(key) && Players[playerIndex].GamepadIndex == -1) return 0;
        if (IsKeyboardKey(key)) return state.Keyboard.IsKeyDown(key).ToInputValue();

        return key switch
        {
            Key.Mouse1 => state.Mouse.LeftButton.ToInputValue(),
            Key.Mouse2 => state.Mouse.RightButton.ToInputValue(),
            Key.Mouse3 => state.Mouse.MiddleButton.ToInputValue(),
            Key.Mouse4 => state.Mouse.XButton1.ToInputValue(),
            Key.Mouse5 => state.Mouse.XButton2.ToInputValue(),
            Key.MouseWheelUp => Math.Max(state.MouseWheelDiff, 0),
            Key.MouseWheelDown => Math.Abs(Math.Min(state.MouseWheelDiff, 0)),
            Key.MouseWheelRight => Math.Max(state.HorizontalMouseWheelDiff, 0),
            Key.MouseWheelLeft => Math.Abs(Math.Min(state.HorizontalMouseWheelDiff, 0)),
            Key.LeftStickRight => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Left.X, playerIndex, true),
            Key.LeftStickLeft => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Left.X, playerIndex, false),
            Key.LeftStickUp => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Left.Y, playerIndex, true),
            Key.LeftStickDown => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Left.Y, playerIndex, false),
            Key.RightStickRight => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Right.X, playerIndex, true),
            Key.RightStickLeft => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Right.X, playerIndex, false),
            Key.RightStickUp => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Right.Y, playerIndex, true),
            Key.RightStickDown => ApplyStickSettings(GetGamePad(state, playerIndex).ThumbSticks.Right.Y, playerIndex, false),
            Key.LeftStickButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.LeftStick).ToInputValue(),
            Key.RightStickButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.RightStick).ToInputValue(),
            Key.GamepadBottomButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.A).ToInputValue(),
            Key.GamepadRightButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.B).ToInputValue(),
            Key.GamepadLeftButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.X).ToInputValue(),
            Key.GamepadTopButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.Y).ToInputValue(),
            Key.GamepadStart => GetGamePad(state, playerIndex).IsButtonDown(Buttons.Start).ToInputValue(),
            Key.GamepadBack => GetGamePad(state, playerIndex).IsButtonDown(Buttons.Back).ToInputValue(),
            Key.GamepadRightShoulder => GetGamePad(state, playerIndex).IsButtonDown(Buttons.RightShoulder).ToInputValue(),
            Key.GamepadLeftShoulder => GetGamePad(state, playerIndex).IsButtonDown(Buttons.LeftShoulder).ToInputValue(),
            Key.GamepadBigButton => GetGamePad(state, playerIndex).IsButtonDown(Buttons.BigButton).ToInputValue(),
            Key.GamepadRightTrigger => GetGamePad(state, playerIndex).Triggers.Right,
            Key.GamepadLeftTrigger => GetGamePad(state, playerIndex).Triggers.Left,
            Key.GamepadDPadRight => GetGamePad(state, playerIndex).IsButtonDown(Buttons.DPadRight).ToInputValue(),
            Key.GamepadDPadLeft => GetGamePad(state, playerIndex).IsButtonDown(Buttons.DPadLeft).ToInputValue(),
            Key.GamepadDPadUp => GetGamePad(state, playerIndex).IsButtonDown(Buttons.DPadUp).ToInputValue(),
            Key.GamepadDPadDown => GetGamePad(state, playerIndex).IsButtonDown(Buttons.DPadDown).ToInputValue(),
            _ => 0,
        };
    }

    /// <summary>
    /// Get <see cref="GamePadState"/> in the <paramref name="state"/> for the <paramref name="playerIndex"/>.
    /// </summary>
    /// <param name="state">State to get from.</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns><see cref="GamePadState"/> in the <paramref name="state"/> for the <paramref name="playerIndex"/>.</returns>
    [Pure]
    public static GamePadState GetGamePad(InputState state, int playerIndex) => state.Gamepads[Players[playerIndex].GamepadIndex];

    /// <summary>
    /// Get player at the specified <paramref name="playerIndex"/>.
    /// </summary>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/></param>
    /// <returns>Player at the specified <paramref name="playerIndex"/>.</returns>
    [Pure]
    public static Player GetPlayer(int playerIndex) => Players[playerIndex];

    /// <summary>
    /// Check whether any button was pressed/released on the gamepad at <paramref name="gamepadIndex"/> compared to previous update.
    /// </summary>
    /// <param name="gamepadIndex">Index of the gamepad in monogame.</param>
    /// <returns>Whether any button was pressed/released on the gamepad at <paramref name="gamepadIndex"/> compared to previous update.</returns>
    [Pure]
    public static bool GamepadStateChanged(int gamepadIndex) => PrevState.Gamepads[gamepadIndex] != CurState.Gamepads[gamepadIndex];

    /// <summary>
    /// Apply setting to the given <paramref name="value"/> of <paramref name="playerIndex"/>'s gamepad stick, for positive values on the axis if <paramref name="positiveValues"/> is <see langword="true"/> or negative ones if <paramref name="positiveValues"/> is <see langword="false"/>.
    /// </summary>
    /// <param name="value">Stick's position, between -1 and 1. If <paramref name="positiveValues"/> is <see langword="true"/>, then negative value is ignored, and vice versa.</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <param name="positiveValues">Whether method should only accept positives values or negative ones.</param>
    /// <returns>Value, after applying settings to it, always non-negative.</returns>
    [Pure]
    public static float ApplyStickSettings(float value, int playerIndex, bool positiveValues)
    {
        if (positiveValues)
        {
            if (value <= 0) return 0;
        }
        else
        {
            if (value >= 0) return 0;
            value = -value;
        }
        InputSettings settings = Players[playerIndex].Settings;
        if (value <= settings.SticksDeadZone) return 0;
        return (value - settings.SticksDeadZone) / (1 - settings.SticksDeadZone); //TODO verify that this works
    }
    #endregion

    #region Checks
    /// <summary>
    /// Check whether the given <paramref name="key"/> is a gamepad button (true) or a keyboard/mouse key (false).
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Whether the given <paramref name="key"/> is a gamepad button (true) or a keyboard/mouse key (false).</returns>
    [Pure]
    public static bool IsGamepadKey(Key key) => key < Key.Mouse5;


    /// <summary>
    /// Check whether the <paramref name="actionIndex"/> is down this frame.
    /// </summary>
    /// <param name="actionIndex">Index of the action in <see cref="ActionNames"/> (the value).</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns>Whether the <paramref name="actionIndex"/> is down this frame.</returns>
    [Pure]
    public static bool ActionDown(int actionIndex, int playerIndex = 0) => Players[playerIndex].Map[actionIndex].Down();

    /// <summary>
    /// Check whether the <paramref name="actionIndex"/> is up this frame.
    /// </summary>
    /// <param name="actionIndex">Index of the action in <see cref="ActionNames"/> (the value).</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns>Whether the <paramref name="actionIndex"/> is down this frame.</returns>
    [Pure]
    public static bool ActionUp(int actionIndex, int playerIndex = 0) => Players[playerIndex].Map[actionIndex].Up();

    /// <summary>
    /// Check whether the <paramref name="actionIndex"/> is pressed this frame.
    /// </summary>
    /// <param name="actionIndex">Index of the action in <see cref="ActionNames"/> (the value).</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns>Whether the <paramref name="actionIndex"/> is down this frame.</returns>
    [Pure]
    public static bool ActionPressed(int actionIndex, int playerIndex = 0) => Players[playerIndex].Map[actionIndex].Pressed();

    /// <summary>
    /// Check whether the <paramref name="actionIndex"/> is released this frame.
    /// </summary>
    /// <param name="actionIndex">Index of the action in <see cref="ActionNames"/> (the value).</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns>Whether the <paramref name="actionIndex"/> is down this frame.</returns>
    [Pure]
    public static bool ActionReleased(int actionIndex, int playerIndex = 0) => Players[playerIndex].Map[actionIndex].Released();

    /// <summary>
    /// Check whether the <paramref name="actionIndex"/> is held this frame.
    /// </summary>
    /// <param name="actionIndex">Index of the action in <see cref="ActionNames"/> (the value).</param>
    /// <param name="playerIndex">Index of the player in <see cref="Players"/>.</param>
    /// <returns>Whether the <paramref name="actionIndex"/> is down this frame.</returns>
    [Pure]
    public static bool ActionHeld(int actionIndex, int playerIndex = 0) => Players[playerIndex].Map[actionIndex].Held();





    /// <summary>
    /// Check whether the <paramref name="key"/> is down this frame.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <param name="playerIndex">Index of the player for whom to check.</param>
    /// <returns>Whether the <paramref name="key"/> is down this frame.</returns>
    [Pure]
    public static bool KeyDown(Key key, int playerIndex = 0) => GetValue(CurState, key, playerIndex) != 0;

    /// <summary>
    /// Check whether the <paramref name="key"/> is up this frame.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <param name="playerIndex">Index of the player for whom to check.</param>
    /// <returns>Whether the <paramref name="key"/> is up this frame.</returns>
    [Pure]
    public static bool KeyUp(Key key, int playerIndex = 0) => GetValue(CurState, key, playerIndex) == 0;


    /// <summary>
    /// Check whether the <paramref name="key"/> is pressed on this frame but wasn't pressed on previous one.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <param name="playerIndex">Index of the player for whom to check.</param>
    /// <returns>Whether the <paramref name="key"/> is pressed on this frame but wasn't pressed on previous one.</returns>
    [Pure]
    public static bool KeyPressed(Key key, int playerIndex = 0) => GetValue(PrevState, key, playerIndex) == 0 && GetValue(CurState, key, playerIndex) != 0;

    /// <summary>
    /// Check whether the <paramref name="key"/> is not pressed on this frame but was pressed on previous one.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <param name="playerIndex">Index of the player for whom to check.</param>
    /// <returns>Whether the <paramref name="key"/> is not pressed on this frame but was pressed on previous one.</returns>
    [Pure]
    public static bool KeyReleased(Key key, int playerIndex = 0) => GetValue(PrevState, key, playerIndex) != 0 && GetValue(CurState, key, playerIndex) == 0;

    /// <summary>
    /// Check whether the <paramref name="key"/> was pressed on this and previous frames.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <param name="playerIndex">Index of the player for whom to check.</param>
    /// <returns>Whether the <paramref name="key"/> was pressed on this and previous frames.</returns>
    [Pure]
    public static bool KeyHeld(Key key, int playerIndex = 0) => GetValue(PrevState, key, playerIndex) != 0 && GetValue(CurState, key, playerIndex) != 0;
    #endregion
}