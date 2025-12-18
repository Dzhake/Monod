using System;
using System.IO;

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
        string[] manifests = Directory.GetFiles(DirectoryPath, "assets.json");
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