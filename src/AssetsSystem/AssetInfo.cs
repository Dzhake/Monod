using System;
using System.Collections.Generic;
using System.IO;
using Monod.AssetsSystem;

namespace Monod;

/// <summary>
/// Struct for storing info required to load an asset: an <see cref="AssetStream"/> and <see cref="PropertiesArray"/> of the asset.
/// </summary>
public readonly struct AssetInfo : IEquatable<AssetInfo>
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
    public readonly Dictionary<int, object>[] PropertiesArray;

    /// <summary>
    /// Path of the asset.
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// Initialize a new instance of <see cref="AssetInfo"/> using the specified <paramref name="assetStream"/> and the <paramref name="propertiesArray"/>.
    /// </summary>
    /// <param name="assetStream"><see cref="Stream"/>, reading the asset.</param>
    /// <param name="type">Type of the asset.</param>
    /// <param name="propertiesArray">Properties dictionary of the asset with key being an <see cref="AssetProps"/> and the value is parsed property.</param>
    /// <param name="path">Path of the asset.</param>
    public AssetInfo(Stream assetStream, AssetType type, Dictionary<int, object>[] propertiesArray, string path)
    {
        AssetStream = assetStream;
        PropertiesArray = propertiesArray;
        Type = type;
        Path = path;
    }

    /// <summary>
    /// Get an asset's property by property's id.
    /// </summary>
    /// <param name="id">Id of the property to get.</param>
    /// <returns>Id of the property if it was found and casted successfully, default(T) otherwise</returns>
    public T? GetProperty<T>(int id)
    {
        foreach (var properties in PropertiesArray)
            if (properties.TryGetValue(id, out object? property))
            {
                if (property is T castedProperty) return castedProperty;
                AssetPropertyTypeMismatchException.Throw(property, Type, typeof(T));
            }
        return default(T);
    }

    /// <inheritdoc />
    public bool Equals(AssetInfo other) => AssetStream.Equals(other.AssetStream) && PropertiesArray.Equals(other.PropertiesArray) && Type.Equals(other.Type);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AssetInfo other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(AssetStream, Type, PropertiesArray);

    /// <summary>
    /// Indicates whether two <see cref="AssetInfo"/> are equal.
    /// </summary>
    /// <param name="left">One <see cref="AssetInfo"/>, to compare with the <paramref name="right"/> one.</param>
    /// <param name="right">Other <see cref="AssetInfo"/>, to compare with the <paramref name="left"/> one.</param>
    /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> <see cref="AssetInfo"/> are equal.</returns>
    public static bool operator ==(AssetInfo left, AssetInfo right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="AssetInfo"/> are not equal.
    /// </summary>
    /// <param name="left">One <see cref="AssetInfo"/>, to compare with the <paramref name="right"/> one.</param>
    /// <param name="right">Other <see cref="AssetInfo"/>, to compare with the <paramref name="left"/> one.</param>
    /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> <see cref="AssetInfo"/> are not equal.</returns>
    public static bool operator !=(AssetInfo left, AssetInfo right) => !left.Equals(right);
}