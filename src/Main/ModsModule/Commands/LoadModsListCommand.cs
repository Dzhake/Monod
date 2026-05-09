namespace Monod.ModsModule.Commands;

public class LoadModsListCommand(ICollection<string> modNames, ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    ///<inheritdoc/>
    public override int TotalProgress => ModManager.TotalTasksThisCommand;
    ///<inheritdoc/>
    public override int CurrentProgress => ModManager.FinishedTasksThisCommand;

    ///<inheritdoc/>
    public override string GetText() => "Loading mods";

    public ICollection<string> ModNames = modNames;

    ///<inheritdoc/>
    public async override Task Run()
    {
        List<string> manifestPaths = new();
        foreach (string modName in ModNames)
            manifestPaths.Add(ModManager.GetModManifestPath(ModManager.FindModDir(modName) ?? ""));
        await ModManager.LoadModsAsync(manifestPaths);
        Finish();
    }
}
