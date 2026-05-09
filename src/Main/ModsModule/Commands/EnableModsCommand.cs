namespace Monod.ModsModule.Commands;

public class EnableModsCommand(ICollection<string> modNames, ModManagerCommandRunner runner) : ModManagerCommand(runner)
{
    ///<inheritdoc/>
    public override int TotalProgress => TotalMods;
    ///<inheritdoc/>
    public override int CurrentProgress => EnabledMods;

    private int EnabledMods;
    private int TotalMods;


    ///<inheritdoc/>
    public override string GetText() => "Loading mods";

    public ICollection<string> ModNames = modNames;

    ///<inheritdoc/>
    public async override Task Run()
    {
        TotalMods += ModNames.Count;
        foreach (string modName in ModNames)
        {
            ModManager.EnableMod(modName);
            EnabledMods++;
        }

        Finish();
    }
}
