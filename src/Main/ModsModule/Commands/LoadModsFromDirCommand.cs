namespace Monod.ModsModule.Commands;

public class LoadModsFromDirCommand(string dir, ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    public override int TotalProgress => ModManager.TotalTasksThisCommand;
    public override int CurrentProgress => ModManager.FinishedTasksThisCommand;

    public override string GetText() => $"Loading mods from {Dir}";

    public string Dir = dir;

    public async override Task Run()
    {
        List<string> manifestPaths = ModManager.FindManifestsInDir(Dir);
        await ModManager.LoadModsAsync(manifestPaths);
        Finish();
    }
}
