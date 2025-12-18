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
    public static readonly JsonSerializerOptions SerializeCommon = new() { AllowTrailingCommas = true};

    /// <summary>
    /// <see cref="SerializeCommon"/> for cases when you want to write clean and readable JSON.
    /// </summary>
    public static readonly JsonSerializerOptions SerializeReadable = new(SerializeCommon) { WriteIndented = true, RespectNullableAnnotations = true};

    /// <summary>
    /// <see cref="SerializeReadable"/>, but includes serializing and deserializing fields.
    /// </summary>
    public static readonly JsonSerializerOptions SerializeWithFields = new(SerializeReadable) { IncludeFields = true };

    
    
    /// <summary>
    /// Common <see cref="JsonSerializerOptions"/> for use when you [de]serialize with <see cref="JsonSerializer"/>.
    /// </summary>
    public static readonly JsonDocumentOptions DocumentCommon = new() { AllowTrailingCommas = true };
}
