using Monod.Utils.General;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Monod.SaveModule;

public static class SaveUtil
{
    public static string GetJsonFilePath(string dir, object _, [CallerArgumentExpression(nameof(_))] string fileName = "")
    {
        return Path.Combine(dir, Path.ChangeExtension(fileName, "json"));
    }

    public static string GetJsonFilePath(string dir, string fileName)
    {
        return Path.Combine(dir, Path.ChangeExtension(fileName, "json"));
    }

    public static T? ReadJson<T>(string filePath)
    {
        if (!File.Exists(filePath)) return default;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), Json.SCommon);
    }

    public static T? ReadJson<T>(string dir, T? obj, [CallerArgumentExpression(nameof(obj))] string fileName = "")
    {
        return ReadJson<T>(GetJsonFilePath(dir, fileName));
    }

    public static void WriteJson(object? obj, string filePath)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(obj, Json.SCommon));
    }

    public static void WriteJson(string dir, object obj, [CallerArgumentExpression(nameof(obj))] string fileName = "")
    {
        WriteJson(obj, GetJsonFilePath(dir, fileName));
    }
}
