using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Monod.ECS.Prefabs;
using Monod.Graphics;
using Monod.Localization;
using Monod.Shared.Extensions;
using Monod.Utils.General;
using ShadowDusk.Compiler;
using ShadowDusk.Core;
using SoLoud;

namespace Monod.AssetsModule;

/// <summary>
/// Class for functions that serve as <see cref="Assets.DefaultParsers"/>.
/// </summary>
public static class AssetParsers
{
    /// <summary>
    /// Parse <see cref="AssetType.Binary"/> as a <see cref="T:byte[]"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static async Task<object?> Binary(AssetInfo info, AssetManager _)
    {
        return await File.ReadAllBytesAsync(info.FullPath);
    }

    /// <summary>
    /// Parse <see cref="AssetType.Text"/> as a <see cref="string"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static async Task<object?> Text(AssetInfo info, AssetManager _)
    {
        return await File.ReadAllTextAsync(info.FullPath);
    }

    /// <summary>
    /// Parse <see cref="AssetType.Image"/> as a <see cref="Texture2D"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static async Task<object?> Image(AssetInfo info, AssetManager _)
    {
        return Texture2D.FromStream(Renderer.device, await info.GetStreamAsync(), DefaultColorProcessors.PremultiplyAlpha);
    }

    /// <summary>
    /// Parse <see cref="AssetType.Audio"/> as a <see cref="SoundEffect"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static async Task<object?> Audio(AssetInfo info, AssetManager assetManager)
    {
        var wav = new Wav();
        int result = wav.load(info.FullPath!);
        if (result != 0)
            Assets.Logger.Error("Failed to load Wav at path {Path} in asset manager {Manager} : {Reason}", info.Path, assetManager.ToString(), MonodGame.soloud.getErrorString(result));
        return wav;
    }

    private static EffectCompiler effectCompiler = new();

    /// <summary>
    /// Lock for <see cref="Effect"/> parser, because creating new Effect internally uses non-concurrent dictionary.
    /// </summary>
    private static Lock EffectLock = new();

    public static async Task<object?> Effect(AssetInfo info, AssetManager assetManager)
    {
        try
        {
            string filePath = Path.Join(assetManager.Loader.DirectoryPath, info.Path).Replace('\\', '/');
            Result<CompiledShader, ShaderError[]> result = await effectCompiler.CompileAsync(await (await info.GetStreamAsync()).ReadStreamAsync(), new CompilerOptions() { SourceFileName = filePath, Target = PlatformTarget.OpenGL });

            if (result.IsFailure)
            {
                foreach (ShaderError e in result.Error)
                    Assets.Logger.Error("{File}({Line},{Column}): {Code}: {Message}", e.File, e.Line, e.Column, e.Code, e.Message);
                return null;
            }

            EffectLock.Enter();
            return new Effect(Renderer.device, result.Value.Data);
        }
        finally
        {
            EffectLock.Exit();
        }
    }


    /// <summary>
    /// Parse <see cref="AssetType.Localization"/> and load it to the <see cref="Locale"/>.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="manager">Manager in which the asset is loaded.</param>
    /// <returns><see langword="null"/>. Instead loads asset to the <see cref="Locale"/>.</returns>
    public static async Task<object?> Localization(AssetInfo info, AssetManager manager)
    {
        //TODO (localization - low priority) Locale.AddManager(manager); -- for localization-only hot reload, because we assume localization is handled by separate asset managers than normal assets, we can only reload asset managers that handle localization
        Locale.Load(new StreamReader(await info.GetStreamAsync()), Path.GetFileNameWithoutExtension(info.Path) == Locale.FallbackLanguage);
        return null;
    }

    /// <summary>
    /// Parse <see cref="AssetType.Font"/> as a <see cref="T:byte[]"/> that represents given asset.
    /// </summary>
    /// <param name="info">Asset info to parse.</param>
    /// <param name="_">Unused.</param>
    /// <returns>Parsed asset.</returns>
    public static async Task<object?> Font(AssetInfo info, AssetManager _)
    {
        return (await info.GetStreamAsync()).ToByteArrayDangerous();
    }

    private static Lock PrefabsLock = new();

    public static async Task<object?> Prefab(AssetInfo info, AssetManager assetManager)
    {
        try
        {
            PrefabsLock.Enter();
            if (!Json.TryDeserialize(await info.GetStreamAsync(), Json.SReadableWithFields, out PrefabAsset? result, out Exception? error))
                Assets.Logger.Error(error, "Failed to deserialize prefab at {Path} in {AssetManager}", info.Path, assetManager);
            return result;
        }
        finally
        {
            PrefabsLock.Exit();
        }
    }
}