using Monod.ModsModule;
using OneOf;

namespace Monod.SaveModule.FilePreset;

public class FilePreset<T> where T : class, new()
{
    public Dictionary<string, T> Presets;
    public T CurrentValue;
    public string CurrentName;
    public string Dir;

    public FilePreset(string dir, string? selectedPreset = null)
    {
        Dir = dir;
        Presets = new();
        if (!string.IsNullOrEmpty(selectedPreset))
            Switch(selectedPreset);
    }

    public void LoadAll(string? selectedPreset = null)
    {
        if (!Directory.Exists(Dir))
        {
            Directory.CreateDirectory(Dir);
            AddDefault();
            return;
        }

        foreach (string file in Directory.EnumerateFiles(Dir, "", SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            T? value = SaveUtil.ReadJson<T>(file);
            if (value is null)
            {
                ModManager.Logger.Warning("Preset at path '{FilePath}' deserialized as null. Verify that the preset is valid and is not empty.", file);
                continue;
            }
            Presets.Add(name, value);
        }

        if (CurrentName is null)
            Switch(selectedPreset ?? ModManager.DEFAULT_PRESET_NAME);
    }

    public void AddDefault()
    {
        string defaultName = "default";
        AddValue(defaultName);
        Switch(defaultName);
    }

    public void SaveAll()
    {
        foreach (T value in Presets.Values) Save(value);
    }

    public void SaveCurrent() => Save(CurrentValue);

    public void Save(T value)
    {
        if (value is not null)
            SaveUtil.WriteJson(value, GetFilePath(CurrentName));
    }

    public OneOf<T, FilePresetError> Switch(string newName)
    {
        SaveCurrent();
        if (!TryGetValue(newName, out var value) || value is null)
        {
            var errOrValue = AddValue(newName);
            if (errOrValue.TryPickT1(out var error, out value))
                return error;
        }

        CurrentName = newName;
        CurrentValue = value;
        return value;
    }

    public OneOf<T?, FilePresetError> Duplicate(string name, string newName)
    {
        if (!Presets.TryGetValue(name, out var value))
            return new PresetNotFoundError(name);
        if (Presets.ContainsKey(newName))
            return new PresetAlreadyExistsError(newName);
        SaveUtil.WriteJson(value, GetFilePath(newName));
        Load(newName, out value);
        return value;
    }

    public OneOf<T, FilePresetError> AddValue(string name, T? value = null)
    {
        if (Presets.ContainsKey(name))
            return new PresetAlreadyExistsError(name);
        value ??= new T();
        Presets[name] = value;
        SaveUtil.WriteJson(value, GetFilePath(name));
        return value;
    }

    public bool TryGetValue(string name, out T? value)
    {
        if (Presets.TryGetValue(name, out value))
            return true;
        if (Load(name, out value))
            return true;
        value = default;
        return false;
    }

    public bool Load(string name, out T? value)
    {
        string filePath = GetFilePath(name);
        if (!File.Exists(filePath))
        {
            value = default;
            return false;
        }
        value = SaveUtil.ReadJson<T>(filePath);
        return true;
    }

    public void Delete(string name)
    {
        Presets.Remove(name);

        if (CurrentName == name)
        {
            AddValue(name);
            SaveCurrent();
        }
        else
        {
            File.Delete(GetFilePath(name));
        }
    }

    public FilePresetError? Rename(string oldName, string newName)
    {
        string oldPath = GetFilePath(oldName);
        if (!File.Exists(oldPath)) return new PresetNotFoundError(oldName);
        string newPath = GetFilePath(newName);
        if (File.Exists(newPath)) return new PresetAlreadyExistsError(newName);

        File.Move(oldPath, newPath, true);
        return null;
    }

    private string GetFilePath(string name)
    {
        return Path.Combine(Dir, Path.ChangeExtension(name, "json"));
    }
}
