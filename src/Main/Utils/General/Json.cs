using Monod.ECS.Prefabs;
using Monod.Graphics.Animations;
using System.Text.Json;

namespace Monod.Utils.General;

/// <summary>
/// Json option presets. See <see cref="JsonUtil"/> for method utilities.
/// </summary>
public static class Json
{
    public static void Initialize()
    {
        JsonSerializerOptions options = new() { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip };
        options.Converters.Add(new EntityConverter());
        options.Converters.Add(new AnimationConverter());
        SCommon = options;

        SReadable = new(SCommon) { WriteIndented = true, RespectNullableAnnotations = true };
        SReadableWithFields = new(SReadable) { IncludeFields = true };

        DCommon = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
    }

    /// <summary>
    /// Common <see cref="JsonSerializerOptions"/> for use when you [de]serialize with <see cref="JsonSerializer"/>.
    /// </summary>
    public static JsonSerializerOptions SCommon;

    /// <summary>
    /// <see cref="SCommon"/> for cases when you want to write clean and readable JSON.
    /// </summary>
    public static JsonSerializerOptions SReadable;

    /// <summary>
    /// <see cref="SReadable"/>, but includes serializing and deserializing fields.
    /// </summary>
    public static JsonSerializerOptions SReadableWithFields;



    /// <summary>
    /// Common <see cref="JsonDocumentOptions"/> for use when you parse with <see cref="JsonDocument"/>.
    /// </summary>
    public static JsonDocumentOptions DCommon;
}
