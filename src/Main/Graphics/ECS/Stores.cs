using Monod.Utils.Enums;
using System.Numerics;

namespace Monod.Graphics.ECS;

public record struct Stores(int inner) : IBitwiseOperators<Stores, Stores, Stores>
{
    public static ExtFlagsInfo<Stores> Info = new();

    public static Stores None = new(0);
    public static Stores All = new(int.MaxValue);


    ///<inheritdoc/>
    public static Stores operator |(Stores left, Stores right) => new(left.inner | right.inner);
    ///<inheritdoc/>
    public static Stores operator ^(Stores left, Stores right) => new(left.inner ^ right.inner);
    ///<inheritdoc/>
    public static Stores operator ~(Stores value) => new(~value.inner);
    ///<inheritdoc/>
    public static Stores operator &(Stores left, Stores right) => new(left.inner & right.inner);
}
