using Monod.Utils.Enums;

namespace Monod.AssetsModule;

public record struct AssetProp(int inner)
{
    public static ExtEnumInfo<AssetProp> Info = new();
}
