using Monod.ModsModule;
using Serilog;
using System.Text.Json;

namespace Monod.SaveModule;

/// <summary>
/// Class for saving and loading data using <see cref="JsonSerializer"/>.
/// </summary>
public static class SaveManager
{
    /// <summary>
    /// <see cref="Directory"/> path where all saves should be located. Directory called "Saves" next to <see cref="AppContext.BaseDirectory"/> by default.
    /// </summary>
    public static string SavesLocation = Environment.GetEnvironmentVariable("SavesDir") ?? Path.Join(AppContext.BaseDirectory, "Saves");

    public static MonodSaveDataProvider MonodProvider = new();
    public static ISaveDataProvider? VanillaProvider;

    public static IEnumerable<ISaveDataProvider> GetProviders()
    {
        yield return MonodProvider;
        if (VanillaProvider is not null) yield return VanillaProvider;
        foreach (Mod mod in ModManager.Mods.Values)
            if (mod.ExternalMod?.SaveDataProvider is not null) yield return mod.ExternalMod.SaveDataProvider;
    }


    public static void Save(SaveType type, string dir)
    {
        foreach (ISaveDataProvider saveProvider in GetProviders())
        {
            try
            {
                saveProvider.Save(type, dir);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to save \"{Name}\" for type \"{Type}\": ", saveProvider.Name, type);
            }
        }
    }

    public static void Load(SaveType type, string dir)
    {
        foreach (ISaveDataProvider saveProvider in GetProviders())
        {
            try
            {
                saveProvider.Load(type, dir);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to load \"{Name}\" for type \"{Type}\": ", saveProvider.Name, type);
            }
        }
    }
}