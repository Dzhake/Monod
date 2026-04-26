namespace Monod.ModsModule.Commands;

public class UnloadModsCommand(ICollection<string> modNames, ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    private int TotalMods;
    private int DisabledMods;
    ///<inheritdoc/>
    public override int TotalProgress => TotalMods;
    ///<inheritdoc/>
    public override int CurrentProgress => DisabledMods;

    ///<inheritdoc/>
    public override string GetText() => "Loading all mods";

    public ICollection<string> ModNames = modNames;

    ///<inheritdoc/>
    public async override Task Run()
    {
        //we assume validation is done earlier

        TotalMods = ModNames.Count;

        foreach (string modName in ModNames)
        {
            ModManager.DisableMod(modName);
            DisabledMods++;
        }

        Finish();
    }
}
