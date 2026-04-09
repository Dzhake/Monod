using Monod.Utils.General;
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

    public static List<ISaveDataProvider> Users;

    public static void Save(string dir, int type)
    {
        List<(string name, object saveObject)> providedData = new();

        foreach (ISaveDataProvider saveUser in Users)
        {
            try
            {
                providedData.Add((saveUser.Name, saveUser.GetSaveObject(type)));
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to get save data for '{Name}' with type '{Type}': ", saveUser.Name, type);
            }
        }

        foreach (var data in providedData)
        {
            string path = Path.Join(dir, data.name);
            try
            {
                string text = JsonSerializer.Serialize(data.saveObject, Json.SReadable);
                File.WriteAllTextAsync(path, text);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to save file at '{Path}': ", path);
            }
        }
    }

    /// <summary>
    /// Deserializes <see cref="File"/> at the specified <paramref name="saveLocation"/> as <typeparamref name="T"/> , and returns the deserialized value, or null if file not found.
    /// </summary>
    /// <param name="saveLocation"><see cref="File"/> path to the file with serialized <typeparamref name="T"/>.</param>
    /// <typeparam name="T">Type of the serialized object.</typeparam>
    /// <returns>Deserialized object.</returns>
    public static T? Load<T>(string saveLocation)
    {
        if (!File.Exists(saveLocation)) return default(T);
        string data = File.ReadAllText(saveLocation);
        return JsonSerializer.Deserialize<T>(data, Json.SReadableWithFields);
    }

    /// <summary>
    /// Serializes <paramref name="data"/> to the <see cref="File"/> at <paramref name="saveLocation"/>.
    /// </summary>
    /// <param name="saveLocation"><see cref="File"/> path where to save the <paramref name="data"/>.</param>
    /// <param name="data">Json-serializable object to save.</param>
    /// <typeparam name="T">Type of the <paramref name="data"/>.</typeparam>
    public static void Save<T>(string saveLocation, T data)
    {
        File.WriteAllText(saveLocation, JsonSerializer.Serialize(data, Json.SReadableWithFields));
    }
}