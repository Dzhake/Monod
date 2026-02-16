using Monod.InputModule.InputActions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.InputModule;

/// <summary>
/// Converts an <see cref="InputMap"/> to or from JSON.
/// </summary>
public class InputMapConverter : JsonConverter<InputMap>
{
    ///<inheritdoc/>
    public override InputMap Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var map = new InputMap();

        var deserializedDict = JsonSerializer.Deserialize<Dictionary<string, InputAction>>(ref reader, options);

        if (deserializedDict is not null)
        {
            foreach (var nameActionPair in deserializedDict)
            {
                // If name is not found, then create it, to keep the value in case the mod is unloaded or something like that.
                if (!Input.ActionNames.TryGetValue(nameActionPair.Key, out int value))
                    value = Input.ActionNames.AddValue(nameActionPair.Key);
                map.Actions[value] = nameActionPair.Value;
            }
        }

        return map;
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, InputMap value, JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, InputAction>();

        foreach (var nameActionPair in value.Actions)
        {
            if (Input.ActionNames.TryGetName(nameActionPair.Key, out string? name) && name != null)
                dict[name] = nameActionPair.Value;
        }

        JsonSerializer.Serialize(writer, dict, options);
    }
}
