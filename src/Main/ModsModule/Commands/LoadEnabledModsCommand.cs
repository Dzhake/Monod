namespace Monod.ModsModule.Commands;

public class LoadEnabledModsCommand(ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    ///<inheritdoc/>
    public override int TotalProgress => ModManager.TotalTasksThisCommand;
    ///<inheritdoc/>
    public override int CurrentProgress => ModManager.FinishedTasksThisCommand;

    ///<inheritdoc/>
    public override string GetText() => "Loading all mods";

    ///<inheritdoc/>
    public async override Task Run()
    {
        List<string> manifestPaths = [];
        foreach (string modName in ModManager.EnabledMods)
        {
            string? dir = ModManager.FindModDir(modName);
            if (dir == null)
            {
                ModManager.ModNotFound();
                Finish();
                return;
            }
            manifestPaths.Add(ModManager.GetModManifestPath(dir));
        }

        await ModManager.LoadModsAsync(manifestPaths);
        Finish();
    }
}
