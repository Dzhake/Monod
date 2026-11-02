using Monod.AssetsModule.Exceptions;
using Monod.AssetsModule.Utils;

namespace Monod.AssetsModule;

/// <summary>
/// Stores info required to load an asset: an <see cref="GetStreamAsync"/> and <see cref="PropertiesArray"/> of the asset.
/// </summary>
public class AssetInfo : IDisposable
{
    /// <summary>
    /// <see cref="Stream"/> reading the asset.
    /// </summary>
    public readonly Stream AssetStream;

    /// <summary>
    /// Type of the asset.
    /// </summary>
    public readonly AssetType Type;

    /// <summary>
    /// Properties of <see cref="MatcherInfo"/>s that asset's path matched to. Use <see cref="GetProperty"/> to get a property.
    /// </summary>
    public Dictionary<AssetProp, object>[]? PropertiesArray;

    /// <summary>
    /// Path of the asset in this asset manager.
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// File path of the asset relative to the <see cref="AssetLoader.DirectoryPath"/>.
    /// </summary>
    public readonly string FilePath;

    /// <summary>
    /// Full path to the file of the asset. SHOULD <b>NOT</b> BE USED FOR PARSING THE ASSET! Use <see cref="AssetStream"/> instead!
    /// </summary>
    public readonly string FullPath;

    public AssetInfo(Stream assetStream, string fullPath, string filePath, string path, AssetType type)
    {
        AssetStream = assetStream;
        FullPath = fullPath;
        FilePath = filePath;
        Path = path;
        Type = type;
    }

    /// <summary>
    /// Get an asset's property by property's id.
    /// </summary>
    /// <param name="id">Id of the property to get.</param>
    /// <returns>Property if it was found and casted successfully, default(T) otherwise.</returns>
    public T? GetProperty<T>(AssetProp id)
    {
        if (PropertiesArray is null) return default;

        foreach (var properties in PropertiesArray)
            if (properties.TryGetValue(id, out object? property))
            {
                if (property is T castedProperty) return castedProperty;
                AssetPropertyTypeMismatchException.Throw(property, Type, typeof(T));
            }

        return default;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        AssetStream?.Dispose();
    }
}