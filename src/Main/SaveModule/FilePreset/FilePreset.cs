using Monod.ModsModule;
using OneOf;

namespace Monod.SaveModule.FilePreset;

public class FilePreset<T> where T : class, new()
{
    public Dictionary<string, T> Values;
    public T CurrentValue;
    public string CurrentName;
    public string Dir;

    public FilePreset(string dir)
    {
        Dir = dir;
        Values = new();
    }

    public void LoadAll()
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
            T? value = SaveManager.ReadJson<T>(file);
            if (value is null)
            {
                ModManager.Logger.Warning("Preset at path {FilePath} deserialized as null. Verify that the preset is valid and is not empty.", file);
                continue;
            }
            Values.Add(name, value);
        }

        Switch("default");
        //TODO save which preset is selected
    }

    public void AddDefault()
    {
        string defaultName = "default";
        AddValue(defaultName);
        Switch(defaultName);
    }

    public void SaveAll()
    {
        foreach (T value in Values.Values) Save(value);
    }

    public void SaveCurrent() => Save(CurrentValue);

    public void Save(T value)
    {
        if (value is not null)
            SaveManager.WriteJson(value, GetFilePath(CurrentName));
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
        if (!Values.TryGetValue(name, out var value))
            return new PresetNotFoundError(name);
        if (Values.ContainsKey(newName))
            return new PresetAlreadyExistsError(newName);
        SaveManager.WriteJson(value, GetFilePath(newName));
        Load(newName, out value);
        return value;
    }

    public OneOf<T, FilePresetError> AddValue(string name, T? value = null)
    {
        if (Values.ContainsKey(name))
            return new PresetAlreadyExistsError(name);
        value ??= new T();
        Values[name] = value;
        SaveManager.WriteJson(value, GetFilePath(name));
        return value;
    }

    public bool TryGetValue(string name, out T? value)
    {
        if (Values.TryGetValue(name, out value))
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
        value = SaveManager.ReadJson<T>(filePath);
        return true;
    }

    public void Delete(string name)
    {
        Values.Remove(name);

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

    private string GetFilePath(string name)
    {
        return Path.Combine(Dir, Path.ChangeExtension(name, "json"));
    }
}
