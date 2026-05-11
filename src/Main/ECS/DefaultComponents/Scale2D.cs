using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Monod.ECS.DefaultComponents;

[ComponentKey("Scale2D")]
[StructLayout(LayoutKind.Explicit)]
[ComponentSymbol("S")]
public struct Scale2D : IComponent, IEquatable<Scale2D>
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(0)] public Vector2 Value;  // 12

    [FieldOffset(0)] public float X;      // (4)
    [FieldOffset(4)] public float Y;      // (4)

    public readonly override string ToString() => $"{X}, {Y}";

    public Scale2D(float x, float y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Scale2D other) => Value == other.Value;
    public static bool operator ==(in Scale2D p1, in Scale2D p2) => p1.Value == p2.Value;
    public static bool operator !=(in Scale2D p1, in Scale2D p2) => p1.Value != p2.Value;

    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object obj) => obj is Scale2D otherPos && otherPos.Equals(this);
}