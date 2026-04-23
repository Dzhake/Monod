namespace Monod.ModsModule.Commands;

public class LoadAllModsCommand(ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    public override int TotalProgress => ModManager.TotalTasksThisCommand;
    public override int CurrentProgress => ModManager.FinishedTasksThisCommand;

    public override string GetText() => "Loading all mods";

    public async override Task Run()
    {
        List<string> manifestPaths = [];
        foreach (string dir in ModManager.GlobalModDirs)
            manifestPaths.AddRange(ModManager.FindManifestsInDir(dir));

        await ModManager.LoadModsAsync(manifestPaths);
        Finish();
    }
}
