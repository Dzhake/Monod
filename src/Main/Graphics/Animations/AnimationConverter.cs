using Monod.AssetsModule;
using Monod.Utils.General;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.Graphics.Animations;

/// <summary>
/// Custom json converter for the <see cref="Animation"/>.
/// </summary>
public class AnimationConverter : JsonConverter<Animation>
{
    ///<inheritdoc/>
    public override Animation Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var animation = new Animation
        {
            Id = root.GetProperty(nameof(Animation.Id)).GetString() ?? "",
            TimeBetweenFrames = root.TryGetProperty(nameof(Animation.TimeBetweenFrames), out var time)
                ? time.GetSingle()
                : 1f,
            PathSuffix = root.TryGetProperty(nameof(Animation.PathSuffix), out var suffix)
                ? suffix.GetString() ?? ""
                : ""
        };

        if (root.TryGetProperty("Frames", out var frames))
        {
            string csv = frames.GetString() ?? "";
            var result = CustomParsers.ParseAnimFrames(csv.AsSpan());

            animation.Frames = result.Match(
                parsed => parsed,
                error =>
                {
                    Assets.Logger.Error("Failed to parse frames for animation with id '{AnimationId}': {Error}", animation.Id, error.Value);
                    return [];
                }
            );
        }
        else
        {
            animation.Frames = [];
        }

        return animation;
    }

    ///<inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        Animation value,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization of Animation is not supported.");
    }
}