using System;
using System.Collections.Generic;
using System.IO;

namespace Monod.AssetsModule;

/// <summary>
/// Stores info directly about the loaded asset: it's <see cref="Stream"/> and <see cref="Type"/>, but unlike <see cref="AssetInfo"/> doesn't store any other info.
/// </summary>
public readonly struct AssetStream : IEquatable<AssetStream>
{
    /// <summary>
    /// <see cref="Stream"/> reading the asset.
    /// </summary>
    public readonly Stream Stream;

    /// <summary>
    /// Type of the asset.
    /// </summary>
    public readonly AssetType Type;

    /// <summary>
    /// Initialize a new instance of <see cref="AssetInfo"/> using the specified <paramref name="assetStream"/>.
    /// </summary>
    /// <param name="assetStream"><see cref="Stream"/>, reading the asset.</param>
    /// <param name="type">Type of the asset.</param>
    public AssetStream(Stream assetStream, AssetType type)
    {
        Stream = assetStream;
        Type = type;
    }

    /// <summary>
    /// Convert this <see cref="AssetStream"/> to an <see cref="AssetInfo"/> with the specified <paramref name="propertiesArray"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="propertiesArray">Properties of <see cref="MatcherInfo"/>s that asset's path matched to.</param>
    /// <param name="path">Path of the asset.</param>
    /// <returns>A new <see cref="AssetInfo"/> with same <see cref="Stream"/> and <see cref="Type"/> and the specified <paramref name="propertiesArray"/>.</returns>
    public AssetInfo ToInfo(Dictionary<int, object>[] propertiesArray, string path) => new AssetInfo(Stream, Type, propertiesArray, path);
    
    /// <inheritdoc />
    public bool Equals(AssetStream other) => Stream.Equals(other.Stream);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AssetStream other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Stream.GetHashCode();

    /// <summary>
    /// Indicates whether two <see cref="AssetInfo"/> are equal.
    /// </summary>
    /// <param name="left">One <see cref="AssetInfo"/>, to compare with the <paramref name="right"/> one.</param>
    /// <param name="right">Other <see cref="AssetInfo"/>, to compare with the <paramref name="left"/> one.</param>
    /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> <see cref="AssetInfo"/> are equal.</returns>
    public static bool operator ==(AssetStream left, AssetStream right) => left.Equals(right);

    /// <summary>
    /// Indicates whether two <see cref="AssetInfo"/> are not equal.
    /// </summary>
    /// <param name="left">One <see cref="AssetInfo"/>, to compare with the <paramref name="right"/> one.</param>
    /// <param name="right">Other <see cref="AssetInfo"/>, to compare with the <paramref name="left"/> one.</param>
    /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> <see cref="AssetInfo"/> are not equal.</returns>
    public static bool operator !=(AssetStream left, AssetStream right) => !left.Equals(right);
}