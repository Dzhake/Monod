using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.InputModule;

/// <summary>
/// Converts an <see cref="KeyMap"/> to or from JSON.
/// </summary>
public class KeyMapConverter : JsonConverter<KeyMap>
{
    ///<inheritdoc/>
    public override KeyMap Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var map = new KeyMap();

        var deserializedDict = JsonSerializer.Deserialize<Dictionary<string, List<Keybind>>>(ref reader, options);

        if (deserializedDict is not null)
        {
            foreach ((string actionName, List<Keybind> keybinds) in deserializedDict)
            {
                // If name is not found, then create it, to keep the value in case the mod is unloaded or something like that.
                int value = Input.ActionNames.AddOrGetValue(actionName);
                map[value] = new(keybinds);
            }
        }

        return map;
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, KeyMap value, JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, List<Keybind>>();

        foreach ((int actionIndex, InputAction action) in value)
        {
            if (Input.ActionNames.TryGetName(actionIndex, out string? name) && name != null)
                dict[name] = action.Keybinds;
        }

        JsonSerializer.Serialize(writer, dict, options);
    }
}
