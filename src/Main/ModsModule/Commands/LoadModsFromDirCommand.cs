namespace Monod.ModsModule.Commands;

public class LoadModsFromDirCommand(string dir, ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    ///<inheritdoc/>
    public override int TotalProgress => ModManager.TotalTasksThisCommand;
    ///<inheritdoc/>
    public override int CurrentProgress => ModManager.FinishedTasksThisCommand;

    ///<inheritdoc/>
    public override string GetText() => $"Loading mods from {Dir}";

    /// <summary>
    /// Directory, where to look for manifests.
    /// </summary>
    public string Dir = dir;

    ///<inheritdoc/>
    public async override Task Run()
    {
        List<string> manifestPaths = ModManager.FindManifestsInDir(Dir);
        await ModManager.LoadModsAsync(manifestPaths);
        Finish();
    }
}
