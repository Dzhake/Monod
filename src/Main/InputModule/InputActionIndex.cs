using Monod.Utils.Enums;

namespace Monod.InputModule;

public record struct InputActionIndex(int Inner)
{
    public static ExtEnumInfo<InputActionIndex> Info = new();

    public static explicit operator InputActionIndex(int i) => new(i);
    public static explicit operator int(InputActionIndex actionIndex) => actionIndex.Inner;
}
