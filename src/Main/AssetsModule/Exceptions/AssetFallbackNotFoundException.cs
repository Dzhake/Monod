namespace Monod.AssetsModule.Exceptions;

/// <summary>
/// The exception that is thrown when <see cref="assetManager"/> could not find fallback for an asset.
/// </summary>
public sealed class AssetFallbackNotFoundException : Exception
{
    /// <summary>
    /// Gets the asset manager that loaded the asset.
    /// </summary>
    private readonly AssetManager assetManager;

    /// <summary>
    /// Gets a relative path to the asset.
    /// </summary>
    private readonly Type FallbackType;

    /// <summary>
    /// Path of the original asset.
    /// </summary>
    private readonly string AssetPath;


    /// <inheritdoc/>
    public override string Message => $"\"{assetManager}\" could not find asset at \"{AssetPath}\" or fallback for type: \"{FallbackType}\"";

    /// <summary>
    ///   <para>Initializes a new instance of the <see cref="AssetFallbackNotFoundException"/> with the specified <see cref="assetManager"/> and <see cref="FallbackType"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager that could not find fallback for the specified <paramref name="fallbackType"/></param>
    /// <param name="fallbackType">Type of the fallback that asset manager tried to find.</param>
    public AssetFallbackNotFoundException(AssetManager assetManager, string assetPath, Type fallbackType)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(assetPath);
        ArgumentNullException.ThrowIfNull(fallbackType);
        this.assetManager = assetManager;
        FallbackType = fallbackType;
        AssetPath = assetPath;
    }
}