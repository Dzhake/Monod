using System;
using System.Diagnostics.CodeAnalysis;
using Monod.AssetsSystem;
using Monod.Utils.Exceptions;

namespace Monod.AssetsSystem;

/// <summary>
/// The exception that is thrown when received asset property's type was not what expected.
/// </summary>
public sealed class AssetPropertyTypeMismatchException : TypeMismatchException
{
    /// <summary>
    /// Property that didn't have the request type.
    /// </summary>
    public readonly object Property;
    
    /// <summary>
    /// Type of the asset that had that property.
    /// </summary>
    public readonly AssetType AssetType;
    
    /// <inheritdoc/>
    public override string Message => $"Mismatch asset property '{Property}' for asset with type '{AssetType}': expected property of type '{ExpectedType}', received '{ReceivedType}'.";

    /// <summary>
    ///   <para>Initialize a new instance of the <see cref="AssetPropertyTypeMismatchException"/>.</para>
    /// </summary>
    /// <param name="property">Property that didn't have the request type.</param>
    /// <param name="assetType">Type of the asset that had that property.</param>
    /// <param name="expectedType">Type of the asset which was requested.</param>
    public AssetPropertyTypeMismatchException(object property, AssetType assetType, Type expectedType) : base(expectedType, property.GetType())
    {
        ArgumentNullException.ThrowIfNull(property);
        Property = property;
        AssetType = assetType;
    }

    /// <summary>
    ///   <para>Initialize a new instance of the <see cref="AssetNotFoundException"/> and throw it.</para>
    /// </summary>
    /// <param name="property">Property that didn't have the request type.</param>
    /// <param name="assetType">Type of the asset that had that property.</param>
    /// <param name="expectedType">Type of the asset which was requested.</param>
    [DoesNotReturn]
    public static void Throw(object property, AssetType assetType, Type expectedType) => throw new AssetPropertyTypeMismatchException(property, assetType, expectedType);
}