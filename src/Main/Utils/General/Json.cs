using Monod.ECS.Prefabs;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Monod.Utils.General;

/// <summary>
/// Small class for caching and reusing <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class Json
{
    /// <summary>
    /// Common <see cref="JsonSerializerOptions"/> for use when you [de]serialize with <see cref="JsonSerializer"/>.
    /// </summary>
    public static readonly JsonSerializerOptions SCommon = GetSCommon();

    private static JsonSerializerOptions GetSCommon()
    {
        JsonSerializerOptions result = new() { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip };
        result.Converters.Add(new EntityConverter());
        return result;
    }

    /// <summary>
    /// <see cref="SCommon"/> for cases when you want to write clean and readable JSON.
    /// </summary>
    public static readonly JsonSerializerOptions SReadable = new(SCommon) { WriteIndented = true, RespectNullableAnnotations = true };

    /// <summary>
    /// <see cref="SReadable"/>, but includes serializing and deserializing fields.
    /// </summary>
    public static readonly JsonSerializerOptions SReadableWithFields = new(SReadable) { IncludeFields = true };



    /// <summary>
    /// Common <see cref="JsonDocumentOptions"/> for use when you parse with <see cref="JsonDocument"/>.
    /// </summary>
    public static readonly JsonDocumentOptions DCommon = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };


    public static bool TryDeserialize<T>(Stream utf8Json, JsonSerializerOptions? options, out T? result, [NotNullWhen(false)] out Exception? exception)
    {
        if (utf8Json?.CanRead != true)
        {
            result = default;
            exception = new Exception("Stream doesn't support reading.");
            return false;
        }

        try
        {
            result = JsonSerializer.Deserialize<T>(utf8Json, options);
            exception = null;
            return true;
        }
        catch (JsonException ex)
        {
            result = default;
            exception = ex;
            return false;
        }
        catch (NotSupportedException ex)
        {
            result = default;
            exception = ex;
            return false;
        }
    }
}
