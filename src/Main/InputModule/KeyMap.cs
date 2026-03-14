using System.Text.Json.Serialization;

namespace Monod.InputModule;

/// <summary>
/// 
/// </summary>
[JsonConverter(typeof(KeyMapConverter))]
public sealed class KeyMap : Dictionary<int, InputAction>
{
    public void Update(InputState state, int playerIndex)
    {
        foreach (InputAction action in Values)
        {
            action.NextFrame();
            action.UpdateIsActive(state, playerIndex);
        }
    }

    public KeyMap Clone()
    {
        KeyMap result = new();

        foreach (var kv in this)
            result.Add(kv.Key, kv.Value.Clone());

        return result;
    }
}

