using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Monod.Graphics;
using Monod.Localization;
using Monod.Shared.Extensions;
using System.Text;

namespace Monod.AssetsModule;

/// <summary>
/// Class for functions that serve as <see cref="Assets.DefaultParsers"/>.
/// </summary>
public static class AssetParsers
{
    /// <summary>
    /// Parse <see cref="AssetType.Binary"/> as a <see cref="byte[]"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static byte[] Binary(AssetInfo info, AssetManager _)
    {
        return info.AssetStream.ToByteArrayDangerous();
    }

    /// <summary>
    /// Parse <see cref="AssetType.Text"/> as a <see cref="string"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static string Text(AssetInfo info, AssetManager _)
    {
        return Assets.ResourcePriority switch
        {
            ResourcePriorityType.Performance => Encoding.UTF8.GetString(info.AssetStream.ToByteArrayDangerous()),
            ResourcePriorityType.Memory => new StreamReader(info.AssetStream, Encoding.UTF8).ReadToEnd(),
            _ => throw new IndexOutOfRangeException()
        };
    }

    /// <summary>
    /// Parse <see cref="AssetType.Image"/> as a <see cref="Texture2D"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static Texture2D Image(AssetInfo info, AssetManager _)
    {
        return Texture2D.FromStream(Renderer.device, info.AssetStream);
    }

    /// <summary>
    /// Parse <see cref="AssetType.Audio"/> as a <see cref="SoundEffect"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static SoundEffect Audio(AssetInfo info, AssetManager _)
    {
        return SoundEffect.FromStream(info.AssetStream);
    }

    /// <summary>
    /// Parse <see cref="AssetType.Effect"/> as a <see cref="Effect"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static Effect Effect(AssetInfo info, AssetManager _)
    {
        return new Effect(Renderer.device, info.AssetStream.ToByteArrayDangerous());
    }

    /// <summary>
    /// Parse <see cref="AssetType.Localization"/> and load it to the <see cref="Locale"/>.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="manager">Manager in which the asset is loaded.</param>
    /// <returns><see langword="null"/>. Instead loads asset to the <see cref="Locale"/>.</returns>
    public static object? Localization(AssetInfo info, AssetManager manager)
    {
        //TODO Locale.AddManager(manager);
        Locale.Load(new StreamReader(info.AssetStream), Path.GetFileNameWithoutExtension(info.Path) == Locale.FallbackLanguage);
        return null;
    }
}