using Linguini.Bundle;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Monod.AssetsSystem;

/// <summary>
///   <para>Defines the types of assets.</para>
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Unknown asset type, not supported by <see cref="AssetsSystem"/>.
    /// </summary>
    Unknown,

    /// <summary>
    /// Type of the asset that should be completely ignored by the <see cref="AssetsSystem"/>. Used to make specific files ignored engine-wide.
    /// </summary>
    Ignore,

    /// <summary>
    /// Binary asset type, converted into <see cref="T:byte[]"/>.
    /// </summary>
    Binary,

    /// <summary>
    /// Text asset type, converted into <see cref="string"/>.
    /// </summary>
    Text,

    /// <summary>
    /// Image asset type, converted into <see cref="Texture2D"/>.
    /// </summary>
    Image,

    /// <summary>
    /// Audio asset type, converted into <see cref="SoundEffect"/>.
    /// </summary>
    Audio,

    /// <summary>
    /// Effect asset type, converted into <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>.
    /// </summary>
    Effect,
    
    /// <summary>
    /// Localization asset type, loaded into <see cref="FluentBundle"/>.
    /// </summary>
    Localization,
}
