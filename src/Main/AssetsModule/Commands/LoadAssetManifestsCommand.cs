using Monod.Shared;
using System.Text.Json;

namespace Monod.AssetsModule.Commands;

/// <summary>
/// Load all asset manifests for the <paramref name="loader"/>.
/// </summary>
/// <param name="loader">Asset loader which executes this command.</param>
public sealed class LoadAssetManifestsCommand(AssetLoader loader) : AssetLoaderCommand(loader)
{
    ///<inheritdoc/>
    public override int TotalProgress => TotalManifests;
    ///<inheritdoc/>
    public override int CurrentProgress => LoadedManifests;

    /// <summary>
    /// Total amount of asset manifests that have been loaded and are to be loaded.
    /// </summary>
    private int TotalManifests;

    /// <summary>
    /// Amount of currently loaded asset manifests.
    /// </summary>
    private int LoadedManifests;

    ///<inheritdoc/>
    public override string GetText() => $"{Loader} is loading asset manifests";

    ///<inheritdoc/>
    public async override Task Run()
    {
        string[] manifests = Directory.GetFiles(Loader.DirectoryPath, Assets.MANIFEST_FILENAME, SearchOption.AllDirectories).Select(path => path.Replace('\\', '/')).ToArray();
        if (manifests.Length == 0)
        {
            OnFinished();
            return;
        }

        TotalManifests = manifests.Length;

        FileWithDepth[] files = new FileWithDepth[manifests.Length];
        for (int i = 0; i < manifests.Length; i++)
            files[i] = new(manifests[i]);
        files.Sort(); //deeper = applies earlier
        List<MatcherInfo> matchers = new();

        foreach (FileWithDepth file in files)
        {
            string manifest = file.FilePath;
            Stream manifestStream = File.OpenRead(manifest);
            matchers.AddRange(ParseAssetManifest(manifestStream, Path.GetRelativePath(Loader.DirectoryPath, Path.GetDirectoryName(manifest) ?? "")));
            LoadedManifests++;
        }

        Loader.Matchers = matchers.ToArray();
        OnFinished();
    }

    /// <summary>
    /// Parse matchers from the specified asset manifest (as a <paramref name="stream"/>) with the specified <paramref name="relativePath"/> of the manifest.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> that reads the asset manifest.</param>
    /// <param name="relativePath">Path of the asset manifest relative to <see cref="AssetManager"/>'s root directory. Used to prefix each match with it, making matchers use subdirectory of the manifest.</param>
    /// <returns>List of <see cref="MatcherInfo"/>s parsed from the specified asset manifest.</returns>
    private static List<MatcherInfo> ParseAssetManifest(Stream stream, string relativePath)
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
}
