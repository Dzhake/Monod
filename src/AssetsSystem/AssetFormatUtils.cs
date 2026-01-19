using System;
using System.Diagnostics.Contracts;
 using System.IO;
 using Monod.AssetsSystem;

 namespace Monod.AssetsSystem;

/// <summary>
/// Util methods related to <see cref="Assets"/> and <see cref="AssetManager"/>s.
/// </summary>
public static class AssetsUtils
{
    /// <summary>
    /// Detects asset format based on the specified <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"><see cref="File"/> path, or file name (might be only extension, but must include period).</param>
    /// <returns>Detected <see cref="AssetType"/>.</returns>
    [Pure] public static AssetType DetectTypeByPath(ReadOnlySpan<char> filePath) => DetectTypeByExtension(Path.GetExtension(filePath));

    /// <summary>
    /// Detects asset format based on the specified <paramref name="extension"/>.
    /// </summary>
    /// <param name="extension">File extension with period (e.g.: ".png").</param>
    /// <returns>Detected <see cref="AssetType"/>. Returns <see cref="AssetType.Unknown"/> for custom formats.</returns>
    [Pure]
    public static AssetType DetectTypeByExtension(ReadOnlySpan<char> extension)
    {
        extension = extension[1..]; //remove the dot
        return extension switch
        {
            "bin" or "bytes" => AssetType.Binary,
            "txt" or "csv" or "json" or "yaml" or "xml" => AssetType.Text,
            "png" or "jpg" or "jpeg" => AssetType.Image,
            "mp3" or "ogg" or "wav" => AssetType.Audio,
            "mgfx" => AssetType.Effect,
            "ftl" => AssetType.Localization,
            _ => AssetType.Unknown,
        };
    }
}