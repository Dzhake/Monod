using Chasm.SemanticVersioning.Ranges;
using Monod.AssetsModule;
using Monod.LogModule;
using Monod.ModsModule.Commands;
using Monod.ModsModule.Exceptions;
using Monod.ModsModule.ModdingOld;
using Monod.SaveModule;
using Monod.SaveModule.FilePreset;
using Monod.Shared.Exceptions;
using Monod.Utils.Collections;
using Monod.Utils.General;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text.Json;

namespace Monod.ModsModule;

/// <summary>
/// Class for loading, unloading and reloading <see cref="Mod"/>s.
/// </summary>
public static class ModManager
{
    private class ModLoadInfo
    {
        public ModManifest manifest;
        public Task task;

        public ModLoadInfo(Task task, ModManifest manifest)
        {
            this.task = task;
            this.manifest = manifest;
        }
    }

    public static readonly string MOD_MANIFEST_FILENAME = "manifest.json";
    public static readonly string RELATIVE_MODS_DIR = Path.Join(AppContext.BaseDirectory, "Mods");

    public static ILogger Logger = LogHelper.ForModule("Mods");

    /// <summary>
    /// Dictionary, where key is mod's name in it's <see cref="ModId"/>, and value is the <see cref="Mod"/> with that name.
    /// </summary>
    public static Dictionary<string, Mod> Mods = new();

    public static List<BrokenMod> BrokenMods = new();

    /// <summary>
    /// Lock for the <see cref="Mods"/> dictionary.
    /// </summary>
    public static ReaderWriterLockSlim ModsLock = new();

    //Serialized
    public static FilePreset<HashSet<string>> EnabledMods;
    public static string SelectedModsPreset;
    public static ConcurrentDictionary<string, string>? ModNameToDirCache;

    public static readonly string DEFAULT_PRESET_NAME = "default";

    public static List<string> GlobalModDirs = [RELATIVE_MODS_DIR];

    public static bool ModsFound = true;

    private static ModManagerCommandRunner Runner = new();
    public static bool InProgress => !Runner.LoadingInactive;
    public static int FinishedTasksThisCommand;
    public static int TotalTasksThisCommand;

    public static void LoadSettings(string saveDir)
    {
        string dir = Path.Combine(saveDir, "ModManager");

        ModNameToDirCache = SaveUtil.ReadJson(dir, ModNameToDirCache) ?? new();

        if (EnabledMods is null)
        {
            string enabledModsDir = Path.Combine(dir, "EnabledMods");
            EnabledMods = new(enabledModsDir);

            string selectedPreset = SaveUtil.ReadJson(dir, SelectedModsPreset) ?? DEFAULT_PRESET_NAME;
            EnabledMods.LoadAll(selectedPreset);
        }
        else
        {
            EnabledMods.LoadAll();
        }
    }

    public static void SaveSettings(string saveDir)
    {
        string dir = Path.Combine(saveDir, "ModManager");
        Directory.CreateDirectory(dir);
        if (ModNameToDirCache is not null)
            SaveUtil.WriteJson(dir, ModNameToDirCache);
        SaveUtil.WriteJson(dir, EnabledMods.CurrentName, nameof(SelectedModsPreset));
        EnabledMods.SaveAll();
    }

    public static async Task LoadModsAsync(List<string> manifestPaths)
    {
        Interlocked.Exchange(ref FinishedTasksThisCommand, 0);
        TotalTasksThisCommand = manifestPaths.Count;
        ObservableDict<string, ModLoadInfo> tasks = new();

        List<Task> loadingManifests = [];
        foreach (string manifestPath in manifestPaths)
            loadingManifests.Add(LoadManifestAndStartLoadingMod(manifestPath, tasks));

        Task.WaitAll(loadingManifests);
        //All mods that could be loaded are loaded by now

        tasks.InvalidCurrentRequests();

        //wait until all mods are loaded
        foreach (object obj in tasks.Values)
            if (obj is Task task) await task;
    }

    public static List<string> FindManifestsInDir(string dir)
    {
        if (!Directory.Exists(dir)) return [];
        if (TryFindManifest(dir, out string manifestPath)) return [manifestPath];

        List<string> manifestPaths = new();

        foreach (string innerDir in Directory.GetDirectories(dir, "", SearchOption.TopDirectoryOnly))
        {
            if (TryFindManifest(innerDir, out manifestPath))
                manifestPaths.Add(manifestPath);
        }

        return manifestPaths;
    }

    public static bool TryFindManifest(string dir, out string manifest)
    {
        manifest = GetModManifestPath(dir);
        return File.Exists(manifest);
    }


    private static async Task LoadManifestAndStartLoadingMod(string manifestPath, ObservableDict<string, ModLoadInfo> loadInfos)
    {
        ModManifest? manifest = await LoadManifestAsync(manifestPath);
        if (manifest is null) //failed to load manifest, broken mod is added in LoadManifest if possible
        {
            Interlocked.Increment(ref FinishedTasksThisCommand);
            return;
        }

        string modDir = Path.GetDirectoryName(manifestPath) ?? "";
        string name = manifest.Id.Name;

        ModNameToDirCache?[name] = modDir;

        if (IsModEnabled(manifest.Id.Name))
        {
            Interlocked.Increment(ref FinishedTasksThisCommand);
            return;
        }


        Task loadingSelf = LoadModFromManifestAsync(manifest, modDir, loadInfos);
        loadInfos.Add(manifest.Id.Name, new(loadingSelf, manifest));
    }

    private static bool IsModLoaded(string modName)
    {
        try
        {
            ModsLock.EnterReadLock();
            return Mods.ContainsKey(modName);
        }
        finally
        {
            ModsLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Check whether mod with the given <paramref name="modName"/> is enabled, and it's version is within the range of <paramref name="acceptedVersions"/>.
    /// </summary>
    /// <param name="modName">Name of the mod.</param>
    /// <param name="acceptedVersions">Range of accepted versions. If mod's version doesn't fall in the range the result will be <see langword="false"/>.</param>
    /// <returns>Whether mod with the given <paramref name="modName"/> is enabled, and it's version is within the range of <paramref name="acceptedVersions"/>.</returns>
    private static bool IsModEnabled(string modName, VersionRange? acceptedVersions = null)
    {
        try
        {
            ModsLock.EnterReadLock();
            return Mods.TryGetValue(modName, out Mod? mod) && mod is not null && (mod.Status == ModStatus.Loading || mod.Status == ModStatus.Enabled) && (acceptedVersions?.IsSatisfiedBy(mod.Manifest.Id.Version) != false);
        }
        finally
        {
            ModsLock.ExitReadLock();
        }
    }

    private static async Task LoadModFromManifestAsync(ModManifest manifest, string modDir, ObservableDict<string, ModLoadInfo> tasks)
    {
        if (!EnabledMods.CurrentPreset.Contains(manifest.Id.Name))
        {
            AddDisabledMod(manifest, modDir);
            Interlocked.Increment(ref FinishedTasksThisCommand);
            return;
        }

        bool depsSatisfied = await WaitUntilDepsLoaded(manifest, tasks);

        if (EnabledMods.CurrentPreset.Contains(manifest.Id.Name) && depsSatisfied)
        {
            Mod mod = CreateModFromManifest(manifest, modDir);
            LoadMod(mod);
            Logger.Information("Loaded mod: {ModName}", mod.GetName());
            AddModToDict(mod);
        }
        else
        {
            AddDisabledMod(manifest, modDir);
        }
        Interlocked.Increment(ref FinishedTasksThisCommand);
    }

    private static async Task<bool> WaitUntilDepsLoaded(ModManifest manifest, ObservableDict<string, ModLoadInfo> tasks)
    {
        if (manifest.Deps is not null)
        {
            foreach (ModDep dep in manifest.Deps)
            {
                if (!EnabledMods.CurrentPreset.Contains(dep.Name))
                    return false;

                if (IsModEnabled(dep.Name, dep.Versions))
                    continue;

                ModLoadInfo depLoadInfo = await tasks.Request(dep.Name);
                if (!dep.Versions.IsSatisfiedBy(depLoadInfo.manifest.Id.Version))
                    return false;

                await depLoadInfo.task;
            }
        }

        return true;
    }

    private static void AddDisabledMod(ModManifest manifest, string modDir)
    {
        Mod disabledMod = CreateModFromManifest(manifest, modDir);
        disabledMod.Status = ModStatus.Disabled;
        AddModToDict(disabledMod);
    }

    public static void LoadMod(Mod mod)
    {
        mod.Status = ModStatus.Loading;
        LoadModData(mod);
        mod.Status = ModStatus.Enabled;
    }


    public static void EnableMod(string modName)
    {
        EnabledMods.CurrentPreset.Add(modName);
    }

    /// <summary>
    /// Load mod manifest at the <paramref name="manifestPath"/> asynchronously.
    /// </summary>
    /// <param name="manifestPath">File path of the mod manifest.</param>
    /// <returns>Loaded manifest, or <see langword="null"/> if manifest failed to load.</returns>
    public static async Task<ModManifest?> LoadManifestAsync(string manifestPath)
    {
        if (!File.Exists(manifestPath))
        {
            Logger.Warning("Tried to load mod manifestPath, but it was not found: \"{Path}\"", manifestPath);
            return null;
        }

        try
        {
            await using FileStream manifestStream = File.OpenRead(manifestPath);
            ModManifest? result = await JsonSerializer.DeserializeAsync<ModManifest>(manifestStream, Json.SCommon);
            if (result is null)
            {
                AddBrokenMod(BrokenMod.FailedToDeserializeManifest(manifestPath));
                return null;
            }
            return result;
        }
        catch (Exception exception)
        {
            AddBrokenMod(BrokenMod.FailedToDeserializeManifest(manifestPath, exception));
            return null;
        }
    }



    public static void LoadModData(Mod mod)
    {
        LoadModAssets(mod);
        LoadModAssembly(mod);
    }

    public static void LoadModAssets(Mod mod)
    {
        string name = mod.GetName();
        if (Assets.ManagerRegistered(name))
        {
            Logger.Warning("Trying to load assets for mod, but manager with this name is already registered: \"{ModName}\"", name);
            return;
        }

        string contentDir = GetContentDirectory(mod.Directory);
        if (Directory.Exists(contentDir))
        {
            mod.Assets = new(new AssetLoader(contentDir));
            Assets.RegisterAssetManager(mod.Assets, name);
        }
    }


    public static Mod CreateModFromManifest(ModManifest manifest, string modDir)
    {
        Mod mod = new();
        mod.AssignManifest(manifest);
        mod.Directory = modDir;
        return mod;
    }

    /// <summary>
    /// Returns manifest file path for the mod.
    /// </summary>
    /// <param name="modDir">Directory, where manifest is located.</param>
    /// <returns>File path of "manifestPath.json".</returns>
    [Pure]
    public static string GetModManifestPath(string modDir) => Path.Join(modDir, MOD_MANIFEST_FILENAME);

    /// <summary>
    /// Get directory path for the specified <paramref name="modDir"/>.
    /// </summary>
    /// <param name="modDir">Directory path of the mod.</param>
    /// <returns>Directory path of the mod's content directory.</returns>
    [Pure]
    public static string GetContentDirectory(string modDir) => Path.Join(modDir, "Content");


    public static string? FindModDir(string modName)
    {
        try
        {
            ModsLock.EnterReadLock();
            if (Mods.TryGetValue(modName, out Mod? mod) && mod is not null)
                return mod.Directory;
        }
        finally
        {
            ModsLock.ExitReadLock();
        }

        if (ModNameToDirCache?.TryGetValue(modName, out string? dir) ?? false) return dir;

        return null;
    }

    public static void ModNotFound()
    {
        ModsFound = false;
        ModNameToDirCache?.Clear();
        EnqueueLoadAllMods();
    }

    public static void DisableMod(string modName)
    {
        ArgumentNullException.ThrowIfNull(modName);

        if (!TryGetMod(modName, out var mod))
            Guard.ArgumentException($"Tried to disable mod \"{modName}\", but it was not found");

        if (mod is null)
            Guard.InvalidOperationException($"Tried to disable mod \"{modName}\", but the mod is null");
        UnloadMod(mod);
    }

    public static void UnloadMod(Mod mod)
    {
        mod.Status = ModStatus.Unloading;

        mod.ExternalMod?.Unload();
        mod.ExternalMod = null;

        mod.HarmonyInstance?.UnpatchSelf();
        mod.HarmonyInstance = null;

        UnloadModAssembly(mod);

        if (mod.Assets is not null)
            Assets.UnRegisterAssetsManager(mod.Assets);
        mod.Assets = null;

        mod.LoggerInstance = null;

        mod.Status = ModStatus.Disabled;
        EnabledMods.CurrentPreset.Remove(mod.GetName());
        Logger.Information("Disabled mod: {ModName}", mod.GetName());
    }

    public static void ReloadModAssembly(Mod mod)
    {
        UnloadModAssembly(mod);
        LoadModAssembly(mod);
    }

    public static void UnloadModAssembly(Mod mod)
    {
        ModAssemblyLoadContext? loadContext = mod.AssemblyContext;
        if (loadContext is not null)
        {
            if (loadContext.MainAssembly is null) return;
            WeakReference<Assembly> weakRef = new(loadContext.MainAssembly);

            for (int i = 0; i < 5; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (!loadContext.Assemblies.Any())
                    break;
            }

            Guard.Exception($"Failed to unload assembly of mod \"{mod.GetName()}. Verify that the mod does required cleanup in ModListener.Unload, does not keep references to classes of types from mod's assembly anywhere, and doesn't run any background tasks.\"");
        }

    }

    public static void LoadModAssembly(Mod mod)
    {
        ModManifest manifest = mod.Manifest;
        if (string.IsNullOrEmpty(manifest.AssemblyFile)) return;
        string dllPath = Path.Join(mod.Directory, manifest.AssemblyFile);
        Logger.Information("Loading assembly from {DllPath}", dllPath);

        ModAssemblyLoadContext? assemblyContext = mod.AssemblyContext;
        if (assemblyContext is null)
            assemblyContext = new(mod);
        if (assemblyContext.Assemblies.Any())
        {
            Logger.Error("Trying to load assembly, but load context already has assemblies loaded");
            return;
        }
        assemblyContext.MainAssembly = LoadAssembly(assemblyContext, dllPath); ;
        mod.AssemblyContext = assemblyContext;
        mod.ExternalMod = ReflectionUtils.CreateInstance<ModListener>(FindModListenerType(assemblyContext.MainAssembly));
        mod.ExternalMod.mod = mod;
        mod.ExternalMod.Initialize();
    }

    /// <summary>
    /// Finds type derived from <see cref="ModListener"/> in the specified <paramref name="assembly"/>, and returns it.
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/>, where to find the type.</param>
    /// <returns>Type derived from <see cref="ModListener"/> in the specified assembly.</returns>
    public static Type FindModListenerType(Assembly assembly)
    {
        Type[] modTypes = assembly.GetExportedTypes().Where(type => type.IsSubclassOf(typeof(ModListener)) && !type.IsAbstract).ToArray();
        switch (modTypes.Length)
        {
            case 0:
                ExternalModCountException.Throw($"Subclass of ModListener not found in {assembly.FullName}", 0, assembly);
                return null;
            case > 1:
                ExternalModCountException.Throw($"Found more than one subclass of ModListener in {assembly.FullName}", modTypes.Length, assembly);
                return null;
            default:
                return modTypes[0];
        }
    }

    private static Assembly LoadAssembly(ModAssemblyLoadContext assemblyContext, string dllPath)
    {
        using FileStream assemblyFileStream = new(dllPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return assemblyContext.LoadFromStream(assemblyFileStream);
    }



    public static void EnqueueLoadEnabledMods()
    {
        Runner.AddCommand(new LoadEnabledModsCommand(Runner));
    }

    public static void EnqueueLoadAllMods()
    {
        Runner.AddCommand(new LoadAllModsCommand(Runner));
    }

    public static void EnqueueLoadModsFromDir(string dir)
    {
        Runner.AddCommand(new LoadModsFromDirCommand(dir, Runner));
    }

    public static void EnqueueToggleMods(ICollection<string> modsToDisable, ICollection<string> modsToEnable)
    {
        EnqueueUnloadMods(modsToDisable);
        EnqueueEnableMods(modsToEnable);
        EnqueueLoadMods(modsToEnable);
    }

    public static void EnqueueEnableMods(ICollection<string> modsToEnable)
    {
        Runner.AddCommand(new EnableModsCommand(modsToEnable, Runner));
    }

    public static void EnqueueToggleMods(ICollection<string> modsToToggle)
    {
        List<string> modsToDisable = new();
        List<string> modsToEnable = new();
        try
        {
            ModsLock.EnterReadLock();
            foreach (string modToToggle in modsToToggle)
            {
                if (Mods.TryGetValue(modToToggle, out var mod) && mod is not null)
                {
                    if (mod.Status is ModStatus.Enabled or ModStatus.Loading)
                        modsToDisable.Add(modToToggle);
                    else
                        modsToEnable.Add(modToToggle);
                }
            }
        }
        finally
        {
            ModsLock.ExitReadLock();
        }

        EnqueueToggleMods(modsToDisable, modsToEnable);
    }

    public static void EnqueueUnloadMods(ICollection<string> modNames)
    {
        Runner.AddCommand(new UnloadModsCommand(modNames, Runner));
    }

    public static void EnqueueLoadMods(ICollection<string> modNames)
    {
        Runner.AddCommand(new LoadModsListCommand(modNames, Runner));
    }



    /// <summary>
    /// Adds the mod to <see cref="Mods"/> in a thread-safe way.
    /// </summary>
    /// <param name="mod">Mod to add.</param>
    public static void AddModToDict(Mod mod)
    {
        try
        {
            ModsLock.EnterWriteLock();
            Mods[mod.GetName()] = mod;
        }
        finally
        {
            ModsLock.ExitWriteLock();
        }
    }

    public static bool TryGetMod(string modName, out Mod? mod)
    {
        try
        {
            ModsLock.EnterReadLock();
            if (Mods.TryGetValue(modName, out mod))
                return true;
            mod = null;
            return false;
        }
        finally
        {
            ModsLock.ExitReadLock();
        }
    }

    private static void AddBrokenMod(BrokenMod brokenMod)
    {
        BrokenMods.Add(brokenMod);
        if (brokenMod.Manifest?.Id.Name is not null)
            Logger.Warning("Broken mod {ModName} : {Issue}", brokenMod.Manifest.Id.Name, brokenMod.FailureReason);
        else
            Logger.Warning("Broken mod at {ManifestPath}: {Issue}", brokenMod.ManifestPath, brokenMod.FailureReason);
    }
}