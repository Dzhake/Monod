using Linguini.Bundle;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Monod.Utils.Enums;

namespace Monod.AssetsModule;

/// <summary>
///   <para>Defines the types of assets.</para>
/// </summary>
public record struct AssetType(int Inner)
{
    /// <summary>
    /// <see cref="ExtEnumInfo{TValue}"/> for the <see cref="AssetType"/>, use it to add new values, retrieve existing ones by name, or retrieve names by value.
    /// </summary>
    public static ExtEnumInfo<AssetType> Info = new();

    public static explicit operator AssetType(int i) => new(i);
    public static explicit operator int(AssetType actionIndex) => actionIndex.Inner;

    /// <summary>
    /// Unknown asset type, not supported by <see cref="AssetsModule"/>.
    /// </summary>
    public static AssetType Unknown = Info.AddOrGetValue(nameof(Unknown));

    /// <summary>
    /// Type of the asset that should be completely ignored by <see cref="AssetsModule"/>. Used to make specific files ignored engine-wide.
    /// </summary>
    public static AssetType Ignore = Info.AddOrGetValue(nameof(Ignore));

    /// <summary>
    /// Binary asset type, converted into <see cref="T:byte[]"/>.
    /// </summary>
    public static AssetType Binary = Info.AddOrGetValue(nameof(Binary));

    /// <summary>
    /// Text asset type, converted into <see cref="string"/>.
    /// </summary>
    public static AssetType Text = Info.AddOrGetValue(nameof(Text));

    /// <summary>
    /// Image asset type, converted into <see cref="Texture2D"/>.
    /// </summary>
    public static AssetType Image = Info.AddOrGetValue(nameof(Image));

    /// <summary>
    /// Audio asset type, converted into <see cref="SoundEffect"/>.
    /// </summary>
    public static AssetType Audio = Info.AddOrGetValue(nameof(Audio));

    /// <summary>
    /// Effect asset type, converted into <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>.
    /// </summary>
    public static AssetType Effect = Info.AddOrGetValue(nameof(Effect));

    /// <summary>
    /// Localization asset type, loaded into <see cref="FluentBundle"/>.
    /// </summary>
    public static AssetType Localization = Info.AddOrGetValue(nameof(Localization));

    /// <summary>
    /// Font asset type, loaded as <see cref="T:byte[]"/>.
    /// </summary>
    public static AssetType Font = Info.AddOrGetValue(nameof(Font));
}
