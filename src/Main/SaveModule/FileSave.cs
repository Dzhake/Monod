namespace Monod.SaveModule;

public class FileSave<T> where T : new()
{
    public string FilePath;
    public T? Value;

    public FileSave(string filePath)
    {
        FilePath = filePath;
    }

    public void Load()
    {
        var result = SaveManager.ReadJson<T>(FilePath);
        if (result is null)
            InitializeDefault();
        else
            Value = result;
    }

    public void Save()
    {
        if (Value is not null) SaveManager.WriteJson(Value, FilePath);
    }

    private void InitializeDefault()
    {
        Value = new();
    }
}
