namespace Monod.AssetsModule;

/// <summary>
/// Represents an object that is stored in the <see cref="AssetLoader.Cache"/>. Not required for assets, but allows them to have some callbacks, that are invoked by the <see cref="AssetLoader"/>.
/// </summary>
public interface IAsset
{
    /// <summary>
    /// Called after the asset is added to the <see cref="AssetLoader.Cache"/>.
    /// </summary>
    public virtual void OnAddedToCache(AssetCacheContext ctx) { }

    /// <summary>
    /// Called immediately before the asset is removed from the <see cref="AssetLoader.Cache"/>. This is recommended for disposing any managed objects. <see cref="IDisposable.Dispose"/> is called right after this, and is recommended for disposing any unmanaged objects.
    /// </summary>
    public virtual void OnRemovedFromCache(AssetCacheContext ctx) { }
}
