using System;
using System.Collections.Generic;
using System.Threading;
using Monod.AssetsSystem;
using Serilog;

namespace Monod.AssetsSystem;

/// <summary>
/// Class for loading, storing, caching, and accessing various assets.
/// </summary>
public sealed class AssetManager : IDisposable
{
    /// <summary>
    /// <see cref="IAssetFilter"/> used for this <see cref="AssetManager"/>.
    /// </summary>
    public readonly IAssetFilter? Filter;

    /// <summary>
    /// <see cref="AssetLoader"/> used for this <see cref="AssetManager"/>.
    /// </summary>
    public readonly AssetLoader Loader;

    

    /// <summary>
    /// Whether this asset manager has been disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Unique prefix for this <see cref="AssetManager"/>, which should be used for <see cref="Assets.Get{T}"/>
    /// </summary>
    public string? Prefix;

    /// <summary>
    /// Whether this <see cref="AssetManager"/> is capable of reloading assets. Should be <see langword="false"/> for <see cref="AssetManager"/> with name "fallbacks", to prevent issues when reloading fallbacks.
    /// </summary>
    public bool CanReload = true;
    
    /// <summary>
    /// Whether this <see cref="AssetManager"/> can reload assets (<see cref="CanReload"/>) and <see cref="MonodMain.HotReload"/> is on.
    /// </summary>
    public bool ShouldReload => CanReload && MonodMain.HotReload;

    /// <summary>
    ///   Get the asset manager's display name.
    /// </summary>
    public string DisplayName => Loader.DisplayName;
    
    /// <summary>
    ///   <para>Returns the string representation of this asset manager: its display name along with the registered prefix.</para>
    /// </summary>
    /// <returns>The string representation of this asset manager.</returns>
    public override string ToString()
        => Prefix is null ? $"(no prefix) {DisplayName}" : $"({Prefix}:/) {DisplayName}";

    public AssetManager(AssetLoader loader, IAssetFilter? filter = null)
    {
        Loader = loader;
        loader.Manager = this;
        Filter = filter;
    }

    /// <summary>
    /// Returns asset at the specified <paramref name="path"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the asset to return. Throws an exception if types of asset in memory and requested types don't match.</typeparam>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/></param>
    /// <returns>Asset at the specified <paramref name="path"/></returns>
    /// <exception cref="AssetTypeMismatchException"><typeparamref name="T"/> does not match type of the loaded asset.</exception>
    public T Get<T>(string path)
    {
        object? asset = Loader.GetAsset(path);
        return asset switch
        {
            null => Assets.NotFoundPolicy switch
            {
                NotFoundPolicyType.Exception => throw new AssetNotFoundException(this, path),
                NotFoundPolicyType.Fallback => GetFallback<T>() ?? throw new AssetFallbackNotFoundException(this, typeof(T)),
                _ => throw new IndexOutOfRangeException($"{nameof(Assets)}.{nameof(Assets.NotFoundPolicy)} was not any known type: {Assets.NotFoundPolicy}")
            },
            T castedAsset => castedAsset,
            _ => throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType())
        };
    }

    /// <summary>
    /// Same as <see cref="Get{T}"/>, but returns null if asset was not found, instead of using <see cref="Assets.NotFoundPolicy"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the asset to return. Throws an exception if types of asset in memory and requested types don't match.</typeparam>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/>.</param>
    /// <returns>Asset at the specified <paramref name="path"/>, or null if not found.</returns>
    /// <exception cref="AssetTypeMismatchException"><typeparamref name="T"/> does not match type of the loaded asset.</exception>
    public T? GetOrDefault<T>(string path)
    {
        object? asset = Loader.GetAsset(path);
        return asset switch
        {
            null => default,
            T castedAsset => castedAsset,
            _ => throw new AssetTypeMismatchException(this, path, typeof(T), asset.GetType())
        };
    }
    
    /// <summary>
    /// Get fallback asset with the specified <typeparamref name="T"/> for asset that could not be found.
    /// </summary>
    /// <typeparam name="T">Type of the fallback.</typeparam>
    /// <returns>Fallback asset for that type.</returns>
    /// <exception cref="AssetFallbackNotFoundException">Thrown if asset fallback was not found, or could not be loaded.</exception>
    public static T? GetFallback<T>()
    {
        return Assets.GetOrDefault<T>($"fallbacks:/{typeof(T)}");
    }
    
    /// <summary>
    /// Loads all assets related to this manager asynchronously, replacing already loaded assets.
    /// </summary>
    public void LoadAssets()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        Loader.LoadAssetManifests();
        Loader.LoadAssets();
        Log.Information("{AssetManager} is loading assets", this);
    }

    /// <summary>
    /// Load asset at the specified path in the cache synchronously, replacing already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public void LoadAsset(string path) => Loader.LoadAsset(path);

    



    /// <inheritdoc/> 
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, true)) return;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   <para>Releases the unmanaged resources used by the asset manager and optionally releases the managed resources.</para>
    /// </summary>
    /// <param name="disposing">Whether to release managed resources too.</param>
    private void Dispose(bool disposing)
    {
        Assets.UnRegisterAssetsManager(this);
        //if (!disposing) return;
    }

    /// <summary>
    /// Deconstructor for <see cref="AssetManager"/>, which doesn't dispose managed resources.
    /// </summary>
    ~AssetManager()
    {
        Dispose(false);
    }
}