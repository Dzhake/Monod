using Monod.Utils.General;
using System.Text.Json;

namespace Monod.SaveModule;

public interface ISaveDataProvider
{
    public string Name { get; }

    public object GetSaveObject(int type);
    public void Load(int type, string json);
    public void InitializeDefault(int type);

    protected object? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Json.SCommon);
}