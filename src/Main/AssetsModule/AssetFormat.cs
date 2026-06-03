using Monod.Utils.Enums;

namespace Monod.AssetsModule;

/// <summary>
/// File format of an asset file.
/// </summary>
/// <param name="Inner">Index of the format in the <see cref="Info"/>.</param>
public record struct AssetFormat(int Inner)
{
    /// <summary>
    /// <see cref="ExtEnumInfo{TValue}"/> for the <see cref="AssetType"/>, use it to add new values, retrieve existing ones by name, or retrieve names by value.
    /// </summary>
    public static ExtEnumInfo<AssetFormat> Info = new();

    public static explicit operator AssetFormat(int i) => new(i);
    public static explicit operator int(AssetFormat actionIndex) => actionIndex.Inner;

    /// <summary>
    /// Unknown/unsupported asset format.
    /// </summary>
    public static AssetFormat Unknown = Info.AddOrGetValue(nameof(Unknown));

    /// <summary>
    /// Binary asset format. Corresponds to <see cref="AssetType.Unknown"/>.
    /// </summary>
    public static AssetFormat Binary = Info.AddOrGetValue(nameof(Binary));

    /// <summary>
    /// Effect source format (plain text). Corresponds to <see cref="AssetType.Ignore"/>, because source shouldn't be loaded. See <see cref="Mgfx"/> for compiled effect.
    /// </summary>
    public static AssetFormat Fx = Info.AddOrGetValue(nameof(Fx));
    /// <summary>
    /// Effect source format header (plain text). Corresponds to <see cref="AssetType.Ignore"/>.
    /// </summary>
    public static AssetFormat Fxh = Info.AddOrGetValue(nameof(Fxh));

    public static AssetFormat Text = Info.AddOrGetValue(nameof(Text));
    public static AssetFormat Csv = Info.AddOrGetValue(nameof(Csv));
    public static AssetFormat Json = Info.AddOrGetValue(nameof(Json));
    public static AssetFormat Yaml = Info.AddOrGetValue(nameof(Yaml));
    public static AssetFormat Xml = Info.AddOrGetValue(nameof(Xml));

    public static AssetFormat Png = Info.AddOrGetValue(nameof(Png));
    public static AssetFormat Jpg = Info.AddOrGetValue(nameof(Jpg));

    public static AssetFormat Mp3 = Info.AddOrGetValue(nameof(Mp3));
    public static AssetFormat Ogg = Info.AddOrGetValue(nameof(Ogg));
    public static AssetFormat Wav = Info.AddOrGetValue(nameof(Wav));

    public static AssetFormat Mgfx = Info.AddOrGetValue(nameof(Mgfx));

    public static AssetFormat Ftl = Info.AddOrGetValue(nameof(Ftl));

    public static AssetFormat Ttf = Info.AddOrGetValue(nameof(Ttf));
}
