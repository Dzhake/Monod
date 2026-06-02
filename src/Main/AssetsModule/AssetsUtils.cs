using System.Diagnostics.Contracts;

namespace Monod.AssetsModule;

/// <summary>
/// Util methods related to <see cref="Assets"/> and <see cref="AssetManager"/>s.
/// </summary>
public static class AssetsUtils
{
    /// <summary>
    /// Detects <see cref="AssetType"/> based on the specified <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath"><see cref="File"/> path, or file name (might be only extension, but must include period).</param>
    /// <returns>Detected <see cref="AssetType"/>.</returns>
    [Pure] public static AssetType DetectTypeByPath(ReadOnlySpan<char> filePath) => DetectTypeByExtension(Path.GetExtension(filePath));

    /// <summary>
    /// Detects <see cref="AssetType"/> based on the specified <paramref name="extension"/>.
    /// </summary>
    /// <param name="extension">File extension with period (e.g.: ".png").</param>
    /// <returns>Detected <see cref="AssetType"/>. Returns <see cref="AssetType.Unknown"/> for custom formats.</returns>
    [Pure]
    public static AssetType DetectTypeByExtension(ReadOnlySpan<char> extension)
    {
        return DetectTypeByFormat(DetectFormatByExtension(extension));
    }

    public static Dictionary<AssetFormat, AssetType> TypeByFormat = new()
    {
        {AssetFormat.Binary, AssetType.Binary },

        {AssetFormat.Fx, AssetType.Ignore },
        {AssetFormat.Fxh, AssetType.Ignore },

        {AssetFormat.Text, AssetType.Text },
        {AssetFormat.Csv, AssetType.Text },
        {AssetFormat.Json, AssetType.Text },
        {AssetFormat.Yaml, AssetType.Text },
        {AssetFormat.Xml, AssetType.Text },

        {AssetFormat.Png, AssetType.Image },
        {AssetFormat.Jpg, AssetType.Image },

        {AssetFormat.Mp3, AssetType.Audio },
        {AssetFormat.Ogg, AssetType.Audio },
        {AssetFormat.Wav, AssetType.Audio },

        {AssetFormat.Mgfx, AssetType.Effect },
        {AssetFormat.Ftl, AssetType.Localization },
        {AssetFormat.Ttf, AssetType.Font },
    };

    [Pure]
    public static AssetType DetectTypeByFormat(AssetFormat format)
    {
        if (TypeByFormat.TryGetValue(format, out var type)) return type;
        return AssetType.Unknown;
    }

    [Pure]
    public static AssetFormat DetectFormatByExtension(ReadOnlySpan<char> extension)
    {
        extension = extension[1..]; //remove the dot
        return extension switch
        {
            "bin" or "bytes" => AssetFormat.Binary,

            "fx" => AssetFormat.Fx,
            "fxh" => AssetFormat.Fxh,

            "txt" => AssetFormat.Text,
            "csv" => AssetFormat.Csv,
            "json" => AssetFormat.Json,
            "yaml" => AssetFormat.Yaml,
            "xml" => AssetFormat.Xml,

            "png" => AssetFormat.Png,
            "jpg" or "jpeg" => AssetFormat.Jpg,

            "mp3" => AssetFormat.Mp3,
            "ogg" => AssetFormat.Ogg,
            "wav" => AssetFormat.Wav,

            "mgfx" => AssetFormat.Mgfx,
            "ftl" => AssetFormat.Ftl,
            "ttf" => AssetFormat.Ttf,
            _ => AssetFormat.Unknown,
        };
    }
}