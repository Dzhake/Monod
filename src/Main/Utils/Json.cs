using System.Text.Json;

namespace Monod.Shared;

/// <summary>
/// Small class for caching and reusing <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class Json
{
    /// <summary>
    /// Common <see cref="JsonSerializerOptions"/> for use when you [de]serialize with <see cref="JsonSerializer"/>.
    /// </summary>
    public static readonly JsonSerializerOptions SCommon = new() { AllowTrailingCommas = true };

    /// <summary>
    /// <see cref="SCommon"/> for cases when you want to write clean and readable JSON.
    /// </summary>
    public static readonly JsonSerializerOptions SReadable = new(SCommon) { WriteIndented = true, RespectNullableAnnotations = true };

    /// <summary>
    /// <see cref="SReadable"/>, but includes serializing and deserializing fields.
    /// </summary>
    public static readonly JsonSerializerOptions SReadableWithFields = new(SReadable) { IncludeFields = true };



    /// <summary>
    /// Common <see cref="JsonSerializerOptions"/> for use when you [de]serialize with <see cref="JsonSerializer"/>.
    /// </summary>
    public static readonly JsonDocumentOptions DCommon = new() { AllowTrailingCommas = true };
}
