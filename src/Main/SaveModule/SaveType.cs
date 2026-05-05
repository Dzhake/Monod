using Monod.Utils.Enums;

namespace Monod.SaveModule;

public record struct SaveType(int inner)
{
    public static ExtEnumInfo<SaveType> Info = new();
    public static SaveType Settings = Info.AddOrGetValue("Settings");
}
