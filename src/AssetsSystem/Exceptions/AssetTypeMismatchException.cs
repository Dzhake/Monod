using System;
using System.Diagnostics.CodeAnalysis;
using Monod.AssetsSystem;
using Monod.Utils.Exceptions;

namespace Monod.AssetsSystem;

/// <summary>
/// The exception that is thrown when received asset's type was not what expected.
/// </summary>
public sealed class AssetTypeMismatchException : TypeMismatchException
{
    /// <summary>
    /// Gets the asset manager that loaded the asset.
    /// </summary>
    public AssetManager AssetManager { get; }

    /// <summary>
    /// Gets a relative path to the asset.
    /// </summary>
    public string RelativePath { get; }

    /// <inheritdoc/>
    public override string Message => $"Mismatch asset type at path {RelativePath} in manager {AssetManager}: expected {ExpectedType}, received {ReceivedType}";

    /// <summary>
    ///   <para>Initialize a new instance of the <see cref="AssetTypeMismatchException"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="relativePath">A relative path to the asset that could not be found.</param>
    /// <param name="expectedType">Type of the asset which was requested.</param>
    /// <param name="receivedType">Type of the asset that was loaded.</param>
    public AssetTypeMismatchException(AssetManager assetManager, string relativePath, Type expectedType, Type receivedType) : base(expectedType, receivedType)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(relativePath);
        AssetManager = assetManager;
        RelativePath = relativePath;
    }

    /// <summary>
    ///   <para>Initialize a new instance of the <see cref="AssetTypeMismatchException"/> and throw it.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="relativePath">A relative path to the asset that could not be found.</param>
    /// <param name="expectedType">Type of the asset which was requested.</param>
    /// <param name="receivedType">Type of the asset that was loaded.</param>
    [DoesNotReturn]
    public static void Throw(AssetManager assetManager, string relativePath, Type expectedType, Type receivedType) => throw new AssetTypeMismatchException(assetManager, relativePath, expectedType, receivedType);
}