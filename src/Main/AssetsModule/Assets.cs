using JetBrains.Annotations;
using Monod.AssetsModule.Commands;
using Monod.Shared.Collections;
using Monod.Shared.Extensions;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace Monod.AssetsModule;

/// <summary>
///   <para>Provides a set of static methods to manage asset managers and load assets.</para>
/// </summary>
public static class Assets
{
    /// <summary>
    /// File name of the file that is considered to be an "asset manifest".
    /// </summary>
    /// <remarks>
    /// This name is reserved and can't be used for assets. "Assets manifest" is a metadata file about assets, containing info such as which <see cref="IAssetFilter"/> and <see cref="AssetParser"/> to use.
    /// </remarks>
    public static readonly string MANIFEST_FILENAME = "assets.json";

    /// <summary>
    /// All registered <see cref="AssetManager"/>s.
    /// </summary>
    public static readonly Dictionary<string, AssetManager> Managers = new();

    /// <summary>
    /// Lock for <see cref="CommandsTotal"/> and <see cref="CommandsFinished"/>.
    /// </summary>
    public static readonly ReaderWriterLockSlim LoadingInfoLock = new();

    /// <summary>
    /// Whether systems should reload assets from the asset cache this frame. Set in <see cref="Update"/>.
    /// </summary>
    public static bool ReloadThisFrame;

    /// <summary>
    /// Whether some asset loader currently reload assets. Used to distinguish loading assets from reloading, to know whether to do reload-only actions.
    /// </summary>
    public static bool Reloading;

    /// <summary>
    /// Whether any asset-loading commands are currently active.
    /// </summary>
    public static bool IsLoading;

    /// <summary>
    /// Total amount of commands in all registered asset managers. Includes finished commands and commands queue. Access only with <see cref="LoadingInfoLock"/>.
    /// </summary>
    public static int CommandsTotal;

    /// <summary>
    /// Amount of commands finished in this load session. Access only with <see cref="LoadingInfoLock"/>.
    /// </summary>
    public static int CommandsFinished;

    /// <summary>
    /// Some command from some asset loader, that can be used for having a quickly moving "numbers go up" progress bar. Chosen in a random-ish way.
    /// </summary>
    public static AssetLoaderCommand? ActiveCommand;

    /// <summary>
    /// Event that is raised when some <see cref="AssetLoader"/>s finish reloading assets. Use <see cref="ReloadThisFrame"/> (recommended) or this to determine when to reload assets.
    /// </summary>
    public static EventBus OnReload = new();

    /// <summary>
    /// Dictionary of asset types and parsers that should be able to parse the specified format. Should be used if the loaded asset doesn't specify some custom parser.
    /// </summary>
    public static readonly Dictionary<AssetType, AssetParser> DefaultParsers = new();

    /// <summary>
    /// Initialize <see cref="Assets"/>. Should be called only once.
    /// </summary>
    public static void Initialize()
    {
        DefaultParsers.AddRange([
            new(AssetType.Binary, AssetParsers.Binary),
            new(AssetType.Text, AssetParsers.Text),
            new(AssetType.Image, AssetParsers.Image),
            new(AssetType.Audio, AssetParsers.Audio),
            new(AssetType.Effect, AssetParsers.Effect),
            new(AssetType.Localization, AssetParsers.Localization),
            new(AssetType.Font, AssetParsers.Font),
        ]);
    }

    /// <summary>
    /// Reset all info about loading with <see cref="LoadingInfoLock"/>.
    /// </summary>
    private static void ResetLoadInfo()
    {
        try
        {
            LoadingInfoLock.EnterWriteLock();
            IsLoading = false;
            CommandsTotal = 0;
            CommandsFinished = 0;
            ActiveCommand = null;
        }
        finally
        {
            LoadingInfoLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Update <see cref="Assets"/>.
    /// </summary>
    public static void Update()
    {
        try
        {
            LoadingInfoLock.EnterUpgradeableReadLock();

            if (!IsLoading)
            {
                foreach (var assetManager in Managers.Values)
                {
                    if (!assetManager.Loader.LoadingInactive)
                    {
                        IsLoading = true;
                        break;
                    }
                }

                if (!IsLoading) return;
            }

            foreach (var assetManager in Managers.Values)
            {
                AssetLoaderCommand? command = assetManager.Loader.ActiveCommand;
                if (command is not null)
                {
                    ActiveCommand = command;
                    break;
                }
            }

            if (CommandsTotal == CommandsFinished)
            {
                Log.Information("Finished executing {Count} commands", CommandsTotal);
                ResetLoadInfo();
            }
        }
        finally
        {
            LoadingInfoLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Increment <see cref="CommandsTotal"/> by 1 in a thread-safe way.
    /// </summary>
    public static void IncrementTotalCommandsCount()
    {
        try
        {
            LoadingInfoLock.EnterWriteLock();
            CommandsTotal++;
        }
        finally
        {
            LoadingInfoLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Increment <see cref="CommandsFinished"/> by 1 in a thread-safe way.
    /// </summary>
    public static void IncrementFinishedCommandsCount()
    {
        try
        {
            LoadingInfoLock.EnterWriteLock();
            CommandsFinished++;
        }
        finally
        {
            LoadingInfoLock.ExitWriteLock();
        }
    }

    /// <summary>
    ///   <para>Adds the specified asset <paramref name="assetManager"/> to the global registry under the specified <paramref name="prefix"/>.</para>
    /// </summary>
    /// <param name="assetManager">The asset manager to register under the specified <paramref name="prefix"/>.</param>
    /// <param name="prefix">The global prefix to register the specified asset <paramref name="assetManager"/> under.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assetManager"/> or <paramref name="prefix"/> is null.</exception>
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
    ///   Get asset at the <paramref name="fullPath"/> from the cache.
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
    ///   <para>Same as <see cref="Get{T}"/>, but returns null if asset was not found, instead of using <see cref="NotFoundPolicy"/>.</para>
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
        path = query[(separatorIndex + 1)..];
    }


    /// <summary>
    /// Invoke all subscribed listeners, to make them reload assets.
    /// </summary>
    public static void EmitReloadEvent() => OnReload.Emit();


    /// <summary>
    /// Whether <see cref="AssetManager"/>s should prefer maximum performance, or lower memory usage. Used in rare cases, where <see cref="ResourcePriorityType.Performance"/> can use a lot of memory.
    /// </summary>
    public static ResourcePriorityType ResourcePriority = ResourcePriorityType.Performance;

    /// <summary>
    /// Action <see cref="AssetManager"/> will do if the specified asset was not found.
    /// </summary>
    public static NotFoundPolicyType NotFoundPolicy = NotFoundPolicyType.Fallback;
}
