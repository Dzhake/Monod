using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Monod.ECS.DefaultComponents;

[ComponentKey("Position2D")]
[StructLayout(LayoutKind.Explicit)]
[ComponentSymbol("P")]
public struct Position2D : IComponent, IEquatable<Position2D>
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(0)] public Vector2 Value;  // 12

    [FieldOffset(0)] public float X;      // (4)
    [FieldOffset(4)] public float Y;      // (4)

    public readonly override string ToString() => $"{X}, {Y}";

    public Position2D(float x, float y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Position2D other) => Value == other.Value;
    public static bool operator ==(in Position2D p1, in Position2D p2) => p1.Value == p2.Value;
    public static bool operator !=(in Position2D p1, in Position2D p2) => p1.Value != p2.Value;

    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object obj) => obj is Position2D otherPos && otherPos.Equals(this);
}