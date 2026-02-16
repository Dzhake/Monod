using Monod.InputModule.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.InputModule.InputActions;

/// <summary>
/// Converts an <see cref="InputAction"/> to or from JSON.
/// </summary>
public class InputActionConverter : JsonConverter<InputAction>
{
    ///<inheritdoc/>
    public override InputAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return InputActionParser.Parse(reader.GetString() ?? "");
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, InputAction value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToString(), options);
    }
}
