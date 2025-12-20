using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Monod.Utils.Collections;
using Monod.Utils.General;
using Serilog;

namespace Monod.AssetsSystem;

/// <summary>
///   <para>Provides a set of static methods to manage asset managers and load assets.</para>
/// </summary>
public static class Assets
{
    /// <summary>
    /// File name of the file that is considered to be an "assets manifest".
    /// </summary>
    /// <remarks>
    /// This name is reserved and can't be used for assets. "Assets manifest" is a metadata file about assets, containing info such as which <see cref="IAssetFilter"/> and <see cref="IAssetParser"/> to use.
    /// </remarks>
    public const string MANIFEST_FILENAME = "assets.json";
        
    /// <summary>
    /// All registered <see cref="AssetManager"/>s.
    /// </summary>
    public static readonly Dictionary<string, AssetManager> Managers = new();
    
    /// <summary>
    /// List of <see cref="AssetLoader"/>s, that are currently reloading some assets. Used to determine whether the game should pause and wait until assets are reloaded.
    /// </summary>
    public static readonly HashSet<AssetLoader> ReloadingAssetLoaders = new();

    /// <summary>
    /// Lock for <see cref="ReloadingAssetLoaders"/>, <see cref="ReloadedAssets"/> and <see cref="TotalReloadingAssets"/>. Required because <see cref="FileSystemWatcher"/> might raise event during the <see cref="Update"/>, causing race condition, leading to <see cref="AssetLoader"/> thinking that reload is currently ongoing even if it's not.
    /// </summary>
    public static readonly ReaderWriterLockSlim ReloadingInfoLock = new();

    /// <summary>
    /// Amount of assets that were reloaded since reload started.
    /// </summary>
    public static int ReloadedAssets;
    
    /// <summary>
    /// Amount of assets that <see cref="ReloadingAssetLoaders"/> want to reload during this reload (including already reloaded).
    /// </summary>
    public static int TotalReloadingAssets;
    
    /// <summary>
    /// Whether systems should reload assets from the asset cache this frame. Set in <see cref="Update"/>.
    /// </summary>
    public static bool ReloadThisFrame;

    /// <summary>
    /// Event that is raised when some <see cref="AssetLoader"/>s finish reloading assets. Use <see cref="ReloadThisFrame"/> (recommended) or this to determine when to reload assets.
    /// </summary>
    public static EventBus OnReload = new();


    /// <summary>
    /// Update <see cref="Assets"/>.
    /// </summary>
    public static void Update()
    {
        ReloadingInfoLock.EnterUpgradeableReadLock();
        try
        {
            if (ReloadingAssetLoaders.Count != 0 && ReloadedAssets == TotalReloadingAssets)
            { //finished reloading
                InvokeReload();
                ReloadThisFrame = true;
                ReloadingInfoLock.EnterWriteLock();
                try
                {
                    ReloadingAssetLoaders.Clear();
                    ReloadedAssets = 0;
                    TotalReloadingAssets = 0;
                }
                finally
                {
                    ReloadingInfoLock.ExitWriteLock();
                }
            }
        }
        finally
        {
            ReloadingInfoLock.ExitReadLock();
        }
        
    }

    /// <summary>
    ///   <para>Adds the specified asset <paramref name="assetManager"/> to the global registry under the specified <paramref name="prefix"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager to register under the specified <paramref name="prefix"/>.</param>
    /// <param name="prefix">The global prefix to register the specified asset <paramref name="assetManager"/> under.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assetManager"/> or <paramref name="prefix"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="assetManager"/> already has a registered prefix, or the specified prefix is already occupied by another asset manager.</exception>
    public static void RegisterAssetManager(AssetManager assetManager, string prefix)
    {
        ArgumentNullException.ThrowIfNull(assetManager);
        ArgumentNullException.ThrowIfNull(prefix);
        if (assetManager.Prefix is not null)
            throw new ArgumentException("The specified manager already has a registered prefix. Check that this manager was unregistered, if it had to.", nameof(assetManager));

        Managers.Add(prefix, assetManager);
        assetManager.Prefix = prefix;
        Log.Information("Registered asset manager with prefix: {Prefix}", prefix);
    }
    
    /// <summary>
    ///   <para>Removes the specified asset <paramref name="assetsManager"/> from the global registry.</para>
    /// </summary>
    /// <param name="assetsManager">The asset manager to remove from the global registry.</param>
    /// <returns><see langword="true"/>, if the specified asset <paramref name="assetsManager"/> was successfully removed; otherwise, <see langword="false"/>.</returns>
    public static bool UnRegisterAssetsManager([NotNullWhen(true)] AssetManager? assetsManager)
    {
        if (assetsManager?.Prefix is { } prefix && Managers.Remove(prefix))
        {
            assetsManager.Prefix = null;
            Log.Information("Unregistered asset manager with prefix: {Prefix}", prefix);
            return true;
        }

        Log.Information("Failed to unregister asset manager: {IAssetsManager}", assetsManager);
        return false;
    }

    /// <summary>
    /// Checks if <see cref="AssetManager"/> with specified <paramref name="prefix"/> is registered.
    /// </summary>
    /// <param name="prefix"><see cref="AssetManager"/>'s prefix.</param>
    /// <returns>Whether <see cref="AssetManager"/> with specified <paramref name="prefix"/> is registered.</returns>
    [Pure]
    public static bool AssetsManagerRegistered(string prefix) => Managers.ContainsKey(prefix);
    

    /// <summary>
    ///   Get asset at the specified <see cref="fullPath"/> from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="fullPath">Full path to the asset (Asset Manager's name + ":" + Relative asset's path)</param>
    /// <returns>Asset from the cache found at the specified path.</returns>
    /// <exception cref="ArgumentException">Could not find asset manager with the specified prefix.</exception>
    [MustUseReturnValue]
    public static T Get<T>(string fullPath)
    {
        SplitPath(fullPath, out var prefix, out var relativePath);
        return GetManager(prefix.ToString()).Get<T>(relativePath.ToString());
    }

    /// <summary>
    ///   <para>Same as <see cref="Get{T}"/>, but returns <see langword="null"/> if asset was not found, instead of using <see cref="Assets.NotFoundPolicy"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="fullPath">Full path to the asset (Asset Manager's name + ":" + Relative asset's path)</param>
    /// <returns>Asset from the cache found at the specified path.</returns>
    /// <exception cref="ArgumentException">Could not find asset manager with the specified prefix.</exception>
    [MustUseReturnValue]
    public static T? GetOrDefault<T>(string fullPath)
    {
        SplitPath(fullPath, out var prefix, out var relativePath);
        return GetManager(prefix.ToString()).GetOrDefault<T>(relativePath.ToString());
    }

    /// <summary>
    /// Get an registered <see cref="AssetManager"/> by it's name from <see cref="Managers"/>. Highly recommended over <see cref="Managers"/>'s indexer for forward-compability. (Maybe we will have some issues with multithreading and this method will do a lock?)
    /// </summary>
    /// <param name="prefix">Name of the manager.</param>
    /// <returns>Manager with the specified name in <see cref="Managers"/>.</returns>
    /// <exception cref="ArgumentException">Could not find asset manager with the specified prefix.</exception>
    [MustUseReturnValue]
    public static AssetManager GetManager(string prefix)
    {
        if (!Managers.TryGetValue(prefix, out AssetManager? manager))
            throw new ArgumentException("Could not find asset manager with the specified prefix. Check that the asset manager was registered and that the prefix is correct.", nameof(prefix));
        return manager;
    }

    /// <summary>
    /// Splits asset path with prefix info prefix and asset path
    /// </summary>
    /// <param name="query">Path to split.</param>
    /// <param name="prefix">Prefix of asset manager.</param>
    /// <param name="path">Asset path for asset manager.</param>
    [Pure]
    public static void SplitPath(ReadOnlySpan<char> query, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> path)
    {
        int separatorIndex = query.IndexOf(":");
        if (separatorIndex == -1)
        {
            //no prefix, only <path>
            prefix = default;
            path = query;
            return;
        }
        // <prefix> ':' <path>
        prefix = query[..separatorIndex];
        path = query[(separatorIndex + 2)..];
    }
    

    /// <summary>
    /// Invoke all subscribed listeners, to make them reload assets.
    /// </summary>
    public static void InvokeReload() => OnReload.Emit();


    /// <summary>
    /// Whether <see cref="AssetManager"/>s should prefer maximum performance, or lower memory usage. Used in rare cases, where <see cref="ResourcePriorityType.Performance"/> can use a lot of memory.
    /// </summary>
    public static ResourcePriorityType ResourcePriority = ResourcePriorityType.Performance;
    
    /// <summary>
    /// Action <see cref="AssetManager"/> will do if the specified asset was not found.
    /// </summary>
    public static NotFoundPolicyType NotFoundPolicy = NotFoundPolicyType.Fallback;

    /// <summary>
    /// <see cref="NamedExtEnum"/> with one value being a name of a property an asset can have and it's unique id. Used to speed up search/parse operations.
    /// </summary>
    public static NamedExtEnum AssetProperties = new();

    /// <summary>
    /// Convert property name of an asset to it's unique Id.
    /// </summary>
    /// <param name="propertyName">Name of the asset's property.</param>
    /// <returns>Unique ID associated with that property.</returns>
    public static int PropNameToId(string propertyName) => AssetProperties.GetValue(propertyName);

    /// <summary>
    /// Parsers for <see cref="AssetProperties"/>. The key is property's id, the value is the parser. Parser accepts the value of the property, and produces some <see cref="object"/> based on it.
    /// </summary>
    public static Dictionary<int, Func<string, object>> AssetPropParsers = new();

    /// <summary>
    /// Parse asset's property form the given string based on property's id/name.
    /// </summary>
    /// <param name="property">Property of the asset from the json, as a string.</param>
    /// <param name="propertyId">Id/name of the property.</param>
    /// <returns>Parsed result.</returns>
    public static object ParseAssetProp(string property, int propertyId) => AssetPropParsers[propertyId](property);
}
