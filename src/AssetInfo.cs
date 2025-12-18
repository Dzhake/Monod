using System.Collections.Generic;
using System.IO;
using Monod.AssetsSystem;

namespace Monod;

/// <summary>
/// Struct for storing info required to load an asset: an <see cref="AssetStream"/> and <see cref="PropertiesArray"/> of the asset.
/// </summary>
public struct AssetInfo
{
    /// <summary>
    /// <see cref="Stream"/> reading the asset.
    /// </summary>
    public Stream AssetStream;
    
    /// <summary>
    /// Properties of <see cref="MatcherInfo"/>s that asset's path matched to. Use <see cref="GetProperty"/> to get a property.
    /// </summary>
    public Dictionary<int, object>[] PropertiesArray;

    /// <summary>
    /// Get an asset's property by property's id.
    /// </summary>
    /// <param name="id">Id of the property, </param>
    /// <returns></returns>
    public object? GetProperty(int id)
    {
        foreach (var properties in PropertiesArray)
            if (properties.TryGetValue(id, out object? property))
                return property;
        return null;
    }
}