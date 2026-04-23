using Monod.ModsModule.Commands;
using Monod.Utils.Commands;

namespace Monod.ModsModule;

/// <summary>
/// Represents <see cref="CommandRunner{T}"/> for <see cref="ModManagerCommand"/>.
/// </summary>
public class ModManagerCommandRunner : CommandRunner<ModManagerCommand>
{
    public void EnqueueLoadAllMods()
    {
        TryAddCommand(new LoadAllModsCommand(this));
    }

    public void LoadModsFromDir(string dir)
    {
        TryAddCommand(new LoadModsFromDirCommand(dir, this));
    }
}
