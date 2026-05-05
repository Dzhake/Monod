namespace Monod.SaveModule;

public interface ISaveDataProvider
{
    public string Name { get; }

    public void Save(SaveType type, string dir);
    public void Load(SaveType type, string dir);
    public void InitializeDefault(SaveType type);
}