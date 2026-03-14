using Microsoft.Xna.Framework.Input;

namespace Monod.InputModule;

public class MonodKeyboardState
{
    /// <summary>
    /// Keys that are currently down, or were pressed and released between this and previous update.
    /// </summary>
    public HashSet<Key> DownKeys;

    /// <summary>
    /// Keys that were released since last state. Might include keys that are currently in <see cref="DownKeys"/>, if the key was pressed and released between update.
    /// </summary>
    public HashSet<Key> ReleasedKeys;

    public MonodKeyboardState(KeyboardState monogameState, MonodKeyboardState previousState)
    {
        DownKeys = monogameState.GetPressedKeys().Select(key => (Key)key).ToHashSet();
        ReleasedKeys = new();
        foreach (Key previouslyDownKey in previousState.DownKeys)
            if (!DownKeys.Contains(previouslyDownKey))
                ReleasedKeys.Add(previouslyDownKey);
    }

    public MonodKeyboardState()
    {
        DownKeys = new();
        ReleasedKeys = new();
    }

    /// <inheritdoc cref="KeyboardState.IsKeyDown(Keys)"/>
    public bool IsKeyDown(Key key) => DownKeys.Contains(key);

    /// <inheritdoc cref="KeyboardState.IsKeyUp(Keys)"/>
    public bool IsKeyUp(Key key) => !DownKeys.Contains(key);
}
