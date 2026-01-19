using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Monod.Utils.General;

namespace Monod.AssetsSystem.AssetLoaders;

/// <summary>
/// Implementation of <see cref="AssetLoader"/> loading assets from the specified <see cref="DirectoryPath"/>.
/// </summary>
public sealed class FileAssetLoader : AssetLoader
{
    /// <summary>
    /// The full path to the directory that this asset loader loads assets from.
    /// </summary>
    public readonly string DirectoryPath;

    /// <inheritdoc />
    public override string DisplayName => $"{DirectoryPath}{Path.DirectorySeparatorChar}**";

    /// <summary>
    /// <see cref="FileSystemWatcher"/>, used to detect changes in files that are stored in <see cref="DirectoryPath"/>.
    /// </summary>
    private FileSystemWatcher? Watcher;

    /// <summary>
    /// Initialize a new <see cref="FileAssetLoader"/> with the specified <see cref="DirectoryPath"/>.
    /// </summary>
    /// <param name="directoryPath">The full path to the directory that this asset loader loads assets from.</param>
    public FileAssetLoader(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        DirectoryPath = Path.GetFullPath(directoryPath);
        if (MonodMain.HotReload) InitializeWatcher();
    }

    /// <summary>
    /// Create and initialize <see cref="Watcher"/> to be active.
    /// </summary>
    private void InitializeWatcher()
    {
        Watcher = new(DirectoryPath);

        Watcher.Changed += OnFileChanged;
        Watcher.Created += OnFileChanged;
        Watcher.Deleted += OnFileDeleted;
        Watcher.Renamed += OnFileRenamed;

        Watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Event for <see cref="Watcher"/> to call when a file's content changes or the file is created.
    /// </summary>
    /// <param name="sender">Event sender, usually <see cref="Watcher"/>.</param>
    /// <param name="e">Arguments of the event.</param>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        ReloadAsset(Path.GetRelativePath(DirectoryPath, e.FullPath));
    }

    /// <summary>
    /// Event for <see cref="Watcher"/> to call when a file is deleted.
    /// </summary>
    /// <param name="sender">Event sender, usually <see cref="Watcher"/>.</param>
    /// <param name="e">Arguments of the event.</param>
    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        RemoveFromCache(Path.GetRelativePath(DirectoryPath, e.FullPath));
    }

    /// <summary>
    /// Event for <see cref="Watcher"/> to call when a file is renamed.
    /// </summary>
    /// <param name="sender">Event sender, usually <see cref="Watcher"/>.</param>
    /// <param name="e">Arguments of the event.</param>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        RemoveFromCache(Path.GetRelativePath(DirectoryPath, e.OldFullPath));
        ReloadAsset(Path.GetRelativePath(DirectoryPath, e.FullPath));
    }

    /// <summary>
    /// Adds asset at the specified path to the reload queue.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    private void ReloadAsset(string path)
    {
        Assets.ReloadQueue?.Add((this, path));
    }

    /// <inheritdoc />
    public override void LoadAssetManifests()
    {
        string[] manifests = Directory.GetFiles(DirectoryPath, Assets.MANIFEST_FILENAME, SearchOption.AllDirectories);
        TotalAssetManifests = manifests.Length;
        
        FileWithDepth[] files = new FileWithDepth[manifests.Length];
        for (int i = 0; i < manifests.Length; i++)
            files[i] = new(manifests[i]);
        files.Sort();
        List<MatcherInfo> matchers = new();
        
        foreach (FileWithDepth file in files)
        {
            string manifest = file.FilePath;
            Stream manifestStream = File.OpenRead(manifest);
            matchers.AddRange(ParseAssetManifest(manifestStream, Path.GetRelativePath(DirectoryPath, Path.GetDirectoryName(manifest) ?? "")));
            LoadedAssetManifests++;
        }

        Matchers = matchers.ToArray();
    }

    /// <inheritdoc />
    public override void LoadAssets()
    {
        string[] assetPaths = FilterPaths(Directory.GetFiles(DirectoryPath, "", SearchOption.AllDirectories).Select(item => Path.GetRelativePath(DirectoryPath, item))).ToArray();
        TotalAssets += assetPaths.Length;
        
        try
        {
            Assets.LoadingInfoLock.EnterWriteLock();
            Assets.LoadingAssetLoaders.Add(this);
            Assets.TotalAssets += assetPaths.Length;
        }
        finally
        {
            Assets.LoadingInfoLock.ExitWriteLock();
        }
        
        foreach (string assetPath in assetPaths)
            MainThread.Add(Task.Run(() => LoadAsset(assetPath)));
    }

    /// <inheritdoc />
    protected override AssetStream? LoadAssetStream(string path)
    {
        string fullPath = Path.Join(DirectoryPath, path);
        if (!File.Exists(fullPath))
            return null;
        return new(File.OpenRead(fullPath), AssetsUtils.DetectTypeByPath(path));
    }
}