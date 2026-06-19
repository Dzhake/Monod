using Monod.Shared.Exceptions;
using Monod.Utils.General;
using System.Reflection;
using System.Runtime.Loader;

namespace Monod.ModsModule.ModdingOld;

/// <summary>
/// <see cref="AssemblyLoadContext"/> for one <see cref="Mod"/>.
/// </summary>
public class ModAssemblyLoadContext : AssemblyLoadContext, IDisposable
{
    /// <summary>
    /// Indicates whether instance is disposing/disposed, to prevent two <see cref="Dispose()"/> calls at same time
    /// </summary>
    private bool disposed;

    /// <summary>
    /// <see cref="FileSystemWatcher"/> which watches for .dll <see cref="File"/>
    /// </summary>
    private readonly FileSystemWatcher? watcher;

    /// <summary>
    /// <see cref="ModManifest"/> for same <see cref="Mod"/> as this <see cref="ModAssemblyLoadContext"/>
    /// </summary>
    private readonly Mod mod;

    public Assembly? MainAssembly;

    /// <summary>
    /// Instances a new <see cref="ModAssemblyLoadContext"/>.
    /// </summary>
    /// <param name="mod">Mod related to the newly created assembly context.</param>
    /// <exception cref="InvalidOperationException"><paramref name="mod.Manifest.AssemblyFile"/> is null.</exception>
    public ModAssemblyLoadContext(Mod mod) : base(isCollectible: true)
    {
        ModManifest manifest = mod.Manifest;
        if (manifest.AssemblyFile is null) Guard.InvalidOperationException("Trying to create ModAssemblyLoadContext for manifest without an AssemblyFile");
        this.mod = mod;

        if (!MonodSettings.HotReload) return;
        watcher = new(Path.Combine(mod.Directory, Path.GetDirectoryName(manifest.AssemblyFile) ?? ""), Path.GetFileName(manifest.AssemblyFile));
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnFileChanged;

        watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (watcher != sender) return;
        //ModManager.ReloadAssembly(mod);
        watcher.EnableRaisingEvents = false; //since assembly is going to be reloaded, this assembly load context will be disposed, so we don't need watcher anymore. Also fixes issue with events being raised twice.
        // TODO (mod manager - low priority) use same load context, only unload the assembly
    }

    /// <summary>
    /// Disposes the <see cref="ModAssemblyLoadContext"/>.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    public void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref disposed, true) || !disposing) return;

        watcher?.Dispose();

        Unload();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}
