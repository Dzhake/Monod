using Monod.ModsModule;

namespace Monod.SaveModule;

public class MonodSaveDataProvider : ISaveDataProvider
{
    public string Name => "Monod";

    public void InitializeDefault(SaveType type)
    {
        if (type == SaveType.Settings)
            InitializeDefaultSettings();
    }

    public void Load(SaveType type, string dir)
    {
        if (type == SaveType.Settings)
            LoadSettings(dir);
    }

    public void Save(SaveType type, string dir)
    {
        if (type == SaveType.Settings)
            SaveSettings(dir);
    }

    public void InitializeDefaultSettings()
    {

    }

    public void SaveSettings(string dir)
    {

    }

    public void LoadSettings(string dir)
    {
        ModManager.LoadSettings(dir);
    }
}
