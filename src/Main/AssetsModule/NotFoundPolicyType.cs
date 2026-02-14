using Monod.AssetsModule.Exceptions;

namespace Monod.AssetsModule;

/// <summary>
/// Types of action <see cref="AssetManager"/> will do if the specified asset was not found.
/// </summary>
public enum NotFoundPolicyType
{
    /// <summary>
    /// Throw <see cref="AssetNotFoundException"/>.
    /// </summary>
    Exception,

    /// <summary>
    /// Fallback to default asset.
    /// </summary>
    Fallback,
}