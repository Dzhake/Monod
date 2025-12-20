using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Monod.Utils.General;

namespace Monod.AssetsSystem.AssetLoaders;

public class FileAssetLoader : AssetLoader
{
    public FileAssetLoader(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        DirectoryPath = Path.GetFullPath(directoryPath);
    }

    /// <summary>
    ///   The full path to the directory that this asset loader loads assets from.
    /// </summary>
    public readonly string DirectoryPath;

    /// <inheritdoc />
    public override string DisplayName => $"{DirectoryPath}{Path.DirectorySeparatorChar}**";

    /// <inheritdoc />
    public override void LoadAssetManifests()
    {
        string[] manifests = Directory.GetFiles(DirectoryPath, Assets.MANIFEST_FILENAME);
        
        FileWithDepth[] files = new FileWithDepth[manifests.Length];
        for (int i = 0; i < manifests.Length; i++)
            files[i] = new(manifests[i]);
        files.Sort();
        List<MatcherInfo> matchers = new();
        
        // Sort by depth: higher depth (more slashes) - lower index, meaning "more specific" manifests will be earlier in the list, so they will be matched earlier and properties from them will be used.
        foreach (FileWithDepth file in files)
        {
            string manifest = file.FilePath;
            Stream manifestStream = File.Open(manifest, FileMode.Open, FileAccess.Read, FileShare.Read);
            string relativePath = Path.GetRelativePath(DirectoryPath, manifest);
            matchers.AddRange(ParseAssetManifest(manifestStream, relativePath));
        }

        Matchers = matchers.ToArray();
    }

    /// <inheritdoc />
    public override void LoadAssets() => throw new System.NotImplementedException();

    /// <inheritdoc />
    public override void LoadAsset(string path) => throw new System.NotImplementedException();

    /// <inheritdoc />
    public override void ReloadAssets() => throw new System.NotImplementedException();

    /// <inheritdoc />
    public override object? GetAsset(string path) => throw new System.NotImplementedException();
}