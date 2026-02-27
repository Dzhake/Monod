using Microsoft.Extensions.FileSystemGlobbing;
using Monod.Shared;
using Serilog;
using System.Text.Json;

namespace Monod.AssetsModule;

/// <summary>
/// Provides methods for loading assets from an external source.
/// </summary>
public abstract class AssetLoader
{
    /// <summary>
    /// Manager that uses this <see cref="AssetLoader"/>.
    /// </summary>
    public AssetManager Manager = null!;

    /// <summary>
    /// Total amount of asset manifests that are loaded or are loading right now.
    /// </summary>
    public int TotalAssetManifests;

    /// <summary>
    /// Amount of currently loaded asset manifests.
    /// </summary>
    public int LoadedAssetManifests;

    /// <summary>
    /// Total amount of assets that are loaded or are loading right now.
    /// </summary>
    public int TotalAssets;

    /// <summary>
    /// Amount of currently loaded assets.
    /// </summary>
    public int LoadedAssets;

    /// <summary>
    /// Whether this <see cref="AssetLoader"/> is currently loading any assets or asset manifests.
    /// </summary>
    //public bool IsLoading => LoadedAssets != TotalAssets || LoadedAssetManifests != TotalAssetManifests;

    /// <summary>
    /// List of <see cref="MatcherInfo"/>s loaded from asset manifests, in order how they should be applied (first one to match means the one to use).
    /// </summary>
    public MatcherInfo[]? Matchers;

    /// <summary>
    /// Cache of the assets, with key being path of the asset in this <see cref="AssetManager"/> and value is the asset.
    /// </summary>
    protected readonly Dictionary<string, object> Cache = new();

    /// <summary>
    /// Lock for <see cref="Cache"/>.
    /// </summary>
    protected ReaderWriterLockSlim CacheLock = new(LockRecursionPolicy.SupportsRecursion);

    protected Queue<Func<Task>> Tasks = new();
    protected Queue<int> TasksPerGroup = new();

    public int ActiveTasksCount;
    public int TotalTasksCount;
    public bool IsLoading => TotalTasksCount != 0;

    public ReaderWriterLockSlim TasksLock = new(LockRecursionPolicy.SupportsRecursion);

    public void ResetTasksCount() => TotalTasksCount = 0;

    /// <inheritdoc />
    public override string ToString() => Manager.ToString();

    protected void ReduceActiveTasks()
    {
        try
        {
            TasksLock.EnterWriteLock();
            ActiveTasksCount--;
            if (ActiveTasksCount == 0) RunTasks();
        }
        finally
        {
            TasksLock.ExitWriteLock();
        }
    }

    protected void TryAddTasks(ICollection<Func<Task>> tasks)
    {
        try
        {
            TasksLock.EnterWriteLock();
            TotalTasksCount += tasks.Count;

            if (ActiveTasksCount == 0)
            {
                foreach (var task in tasks) MainThread.Add(task());
                return;
            }

            foreach (var task in tasks) Tasks.Enqueue(task);
            TasksPerGroup.Enqueue(tasks.Count);
        }
        finally
        {
            TasksLock.ExitWriteLock();
        }
    }

    protected void RunTasks()
    {
        try
        {
            TasksLock.EnterWriteLock();
            if (Tasks.Count == 0) return;
            int tasksAmount;
            if (TasksPerGroup.Count == 0) tasksAmount = Tasks.Count;
            else tasksAmount = TasksPerGroup.Dequeue();

            ActiveTasksCount += tasksAmount;
            for (int i = 0; i < tasksAmount; i++)
                MainThread.Add(Tasks.Dequeue().Invoke());
        }
        finally
        {
            TasksLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Load all asset manifests for this <see cref="AssetLoader"/> asynchronously, can be used for reloading.
    /// </summary>
    public abstract void LoadAssetManifests();

    /// <summary>
    /// Parse matchers from the specified asset manifest (as a <paramref name="stream"/>) with the specified <paramref name="relativePath"/> of the manifest.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> that reads the asset manifest.</param>
    /// <param name="relativePath">Path of the asset manifest relative to <see cref="AssetManager"/>'s root directory. Used to prefix each match with it, making matchers use subdirectory of the manifest.</param>
    /// <returns>List of <see cref="MatcherInfo"/>s parsed from the specified asset manifest.</returns>
    protected static List<MatcherInfo> ParseAssetManifest(Stream stream, string relativePath)
    {
        var document = JsonDocument.Parse(stream, Json.DCommon);

        List<MatcherInfo> result = new();

        foreach (JsonProperty match in document.RootElement.EnumerateObject())
        {
            var properties = new Dictionary<int, object>();

            foreach (JsonProperty inner in match.Value.EnumerateObject())
            {
                int id = AssetProps.NameToId(inner.Name);
                properties[id] = AssetProps.ParseAssetProp(inner.Value.GetRawText(), id);
            }

            result.Add(new MatcherInfo(Globbing.MatcherFromString(match.Name, relativePath), properties));
        }

        return result;
    }

    /// <summary>
    /// Matches the given <paramref name="path"/> against <see cref="Matchers"/>.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/>.</param>
    /// <returns>List of properties the asset at the given <paramref name="path"/> should have.</returns>
    public List<Dictionary<int, object>> MatchPath(string path)
    {
        List<Dictionary<int, object>> result = new();
        if (Matchers is null) return result;
        foreach (MatcherInfo matcherInfo in Matchers)
            if (matcherInfo.PathMatcher.Match(path).HasMatches) result.Add(matcherInfo.Properties);
        return result;
    }

    /// <summary>
    /// Matches the given <paramref name="path"/> against <see cref="Matchers"/> and returns the result as an array.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/>.</param>
    /// <returns>List of properties the asset at the given <paramref name="path"/> should have, as an array.</returns>
    public Dictionary<int, object>[] MatchPathAsArray(string path) => MatchPath(path).ToArray();

    /// <summary>
    /// Load all assets in the cache asynchronously, <b>replacing</b> already loaded assets.
    /// </summary>
    public void ReloadAssets()
    {
        if (IsLoading) return;

        LoadedAssets = 0;
        TotalAssets = 0;
        try
        {
            CacheLock.EnterWriteLock();
            foreach (object asset in Cache.Values)
                if (asset is IDisposable disposable) disposable.Dispose();
            Cache.Clear();
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }

        LoadAssets();
    }

    /// <summary>
    /// Load all assets from the <paramref name="dirPath"/>.
    /// </summary>
    /// <param name="dirPath"></param>
    public abstract void LoadAssetsInDir(string dirPath);

    /// <summary>
    /// Load all assets in the cache asynchronously, <b>replacing</b> already loaded assets.
    /// </summary>
    public void LoadAssets() => LoadAssetsInDir("");

    /// <summary>
    /// Filter asset paths for <see cref="LoadAssets"/> using <see cref="AssetManager.Filter"/> and filtering entries that are already in the <see cref="Cache"/>.
    /// </summary>
    /// <param name="assetPaths"><see cref="IEnumerable{string}"/> containing asset paths in this asset manager.</param>
    /// <returns><paramref name="assetPaths"/> filtered using <see cref="AssetManager.Filter"/> and without entries that are already in the <see cref="Cache"/>.</returns>
    protected IEnumerable<string> FilterPaths(IEnumerable<string> assetPaths)
    {
        Func<string, bool> filter;
        if (Manager.Filter is null)
            filter = path => !Cache.ContainsKey(path);
        else
            filter = path => !Cache.ContainsKey(path) && Manager.Filter.ShouldLoad(path);

        try
        {
            CacheLock.EnterReadLock();
            return assetPaths.Where(filter);
        }
        finally
        {
            CacheLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Load asset at the specified path in the cache synchronously, <b>replacing</b> already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public void LoadAsset(string path)
    {
        AssetStream? assetStream = LoadAssetStream(path);
        if (assetStream is null || assetStream.Value.Type == AssetType.Ignore)
        {
            RemoveFromCache(path); //When asset was deleted/renamed.
            AddLoadingAssetsCount();
            return;
        }

        AssetInfo assetInfo = assetStream.Value.ToInfo(MatchPathAsArray(path), path);
        AssetParser? parser = GetParser(assetInfo);
        if (parser is null)
        {
            Log.Warning("Could not find parser for the asset with type {Type} at path {Path}. Verify that parser is specified correctly and that asset has a supported format.", assetInfo.Type, path);
            AddLoadingAssetsCount();
            return;
        }

        object? asset = parser(assetInfo, Manager);
        if (asset is null) //failed to load (logged by parser)/loaded somewhere else by parser
        {
            AddLoadingAssetsCount();
            return;
        }

        LoadIntoCache(path, asset);
        AddLoadingAssetsCount();
        assetInfo.AssetStream.Close();
    }

    private void AddLoadingAssetsCount()
    {
        Interlocked.Add(ref LoadedAssets, 1);

        try
        {
            Assets.LoadingInfoLock.EnterWriteLock();
            if (Assets.LoadingAssetLoaders.Contains(this)) Assets.LoadedAssets++;
        }
        finally
        {
            Assets.LoadingInfoLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Get <see cref="AssetParser"/> based on the specified <paramref name="assetInfo"/>.
    /// </summary>
    /// <param name="assetInfo">Info about asset to get parser for.</param>
    /// <returns><see cref="AssetParser"/> based on the specified <paramref name="assetInfo"/> or null if parser could not be determined.</returns>
    private static AssetParser? GetParser(AssetInfo assetInfo)
    {
        AssetParser? parser = assetInfo.GetProperty<AssetParser>(AssetProps.Parser);
        if (parser is null) Assets.DefaultParsers.TryGetValue(assetInfo.Type, out parser);
        return parser;
    }

    /// <summary>
    /// Load the specified <paramref name="asset"/> into the cache with the <paramref name="path"/> as key.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <param name="asset">Asset to load into the cache.</param>
    protected void LoadIntoCache(string path, object asset)
    {
        try
        {
            CacheLock.EnterWriteLock();
            if (Cache.TryGetValue(path, out object? oldAsset) && oldAsset is IDisposable disposable) disposable.Dispose();
            Cache[path] = asset;
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Remove the specified key (path) from the <see cref="Cache"/>.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    public void RemoveFromCache(string path)
    {
        try
        {
            CacheLock.EnterWriteLock();
            if (Cache.TryGetValue(path, out object? asset) && asset is IDisposable disposable) disposable.Dispose();
            Cache.Remove(path);
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }

        Log.Information("{This}: Unloaded asset at {Path}", this, path);
    }

    /// <summary>
    /// Remove all assets in the <paramref name="dirPath"/> from cache (unload them).
    /// </summary>
    /// <param name="dirPath">Directory path in this <see cref="AssetLoader"/>.</param>
    public void RemoveDirFromCache(string dirPath)
    {
        Interlocked.Add(ref TotalAssets, 1);
        if (!dirPath.EndsWith(Path.DirectorySeparatorChar) && !dirPath.EndsWith(Path.AltDirectorySeparatorChar)) dirPath += Path.DirectorySeparatorChar;

        try
        {
            int unloadedAssetsCount = 0;
            CacheLock.EnterWriteLock();
            foreach (string asset in Cache.Keys)
            {
                if (asset.StartsWith(dirPath))
                {
                    Cache.Remove(asset);
                    unloadedAssetsCount++;
                }
            }

            Interlocked.Add(ref TotalAssets, -unloadedAssetsCount);
            Interlocked.Add(ref LoadedAssets, -unloadedAssetsCount);
            Log.Information("{This}: Unloaded {Count} assets", this, unloadedAssetsCount);
            Cache.TrimExcess();
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Remove all <paramref name="paths"/> from <see cref="Cache"/>.
    /// </summary>
    /// <param name="paths">Array of asset paths in this <see cref="AssetLoader"/>.</param>
    public void RemoveFromCache(string[] paths)
    {
        try
        {
            CacheLock.EnterWriteLock();
            foreach (string path in paths)
                Cache.Remove(path);
            Cache.TrimExcess();
            GC.Collect();
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
        Log.Information("{This}: Unloaded {Count} assets", this, paths.Length);
    }

    /// <summary>
    /// Load stream of the asset at the specified path. Loads new asset each time.
    /// </summary>
    /// <param name="path">Path of the asset to load in this <see cref="AssetLoader"/>.</param>
    /// <returns>New <see cref="AssetStream"/> with the <see cref="AssetStream.Stream"/> reading the asset and the type respective to the loaded asset, or null if the asset was not found.</returns>
    protected abstract AssetStream? LoadAssetStream(string path);

    /// <summary>
    /// Load stream of the asset at the specified path asynchronously. Loads new asset each time.
    /// </summary>
    /// <param name="path">Path of the asset to load in this <see cref="AssetLoader"/>.</param>
    /// <returns>New <see cref="AssetStream"/> with the <see cref="AssetStream.Stream"/> reading the asset and the type respective to the loaded asset, or null if the asset was not found.</returns>
    protected Task<AssetStream?> LoadAssetStreamAsync(string path) => new(() => LoadAssetStream(path));

    /// <summary>
    /// Get asset at the specified path from the cache.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <returns>The asset, or null if asset at the specified <paramref cref="path"/> was not found.</returns>
    public object? GetAsset(string path)
    {
        try
        {
            CacheLock.EnterReadLock();
            return Cache.GetValueOrDefault(path);
        }
        finally
        {
            CacheLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Check whether <paramref name="path"/> is a directory.
    /// </summary>
    /// <param name="path">Path of the potential directory in this <see cref="AssetLoader"/>.</param>
    /// <returns>Whether <paramref name="path"/> is a directory.</returns>
    public abstract bool IsDir(string path);
}