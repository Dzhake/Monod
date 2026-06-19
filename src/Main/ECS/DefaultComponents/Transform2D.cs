using Friflo.Json.Fliox;
using System.Runtime.InteropServices;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Monod.ECS.DefaultComponents;

[ComponentKey("Transform2D")]
[StructLayout(LayoutKind.Explicit)]
[ComponentSymbol("T")]
public record struct Transform2D : IComponent, IEquatable<Transform2D>
{
    [Browse(Never)]
    [Ignore]
    [FieldOffset(0)] public Vector2 Position;  // 8

    [FieldOffset(0)] public float PosX;
    [FieldOffset(4)] public float PosY;

    [Browse(Never)]
    [Ignore]
    [FieldOffset(8)] public Vector2 Scale; // 8
    [FieldOffset(8)] public float ScaleX;
    [FieldOffset(12)] public float ScaleY;

    /// <summary>
    /// Whether entity is facing right (false) or left (true).
    /// </summary>
    [FieldOffset(16)] public bool FlipX; // 4

    /// <summary>
    /// Whether entity is facing down (false) or true (true).
    /// </summary>
    [FieldOffset(20)] public bool FlipY; // 4

    /// <summary>
    /// Rotation in radians, clockwise.
    /// </summary>
    [FieldOffset(24)] public float Rotation;

    public readonly override string ToString() => $"{{ Position: {PosX}, {PosY}; Scale: {ScaleX}, {ScaleY}; Flip: {FlipX}, {FlipY}; Rotation: {Rotation} }}";

    public Transform2D(float posX, float posY, float scaleX = 1, float scaleY = 1, bool flipX = false, bool flipY = false, float rotation = 0)
    {
        PosX = posX;
        PosY = posY;
        ScaleX = scaleX;
        ScaleY = scaleY;
        FlipX = flipX;
        FlipY = flipY;
        Rotation = rotation;
    }

    /// <summary>
    /// Created a new instance of <see cref="Transform2D"/> with specified values.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="scale">Scale of the entity, <see cref="Vector2.One"/> by default.</param>
    /// <param name="flipX">Whether entity is facing right (false) or left (true).</param>
    /// <param name="flipY">Whether entity is facing down (false) or true (true).</param>
    /// <param name="rotation"></param>
    public Transform2D(Vector2 position, Vector2? scale = null, bool flipX = false, bool flipY = false, float rotation = 0)
    {
        Position = position;
        Scale = scale ?? Vector2.One;
        FlipX = flipX;
        FlipY = flipY;
        Rotation = rotation;
    }

    /// <summary>
    /// Get rotation after appling <see cref="FlipX"/> and <see cref="FlipY"/>.
    /// </summary>
    /// <returns>Rotation after appling <see cref="FlipX"/> and <see cref="FlipY"/>.</returns>
    public float GetFlippedRotation()
    {
        if (FlipX && FlipY) return Rotation + MathF.PI;
        if (FlipX) return MathF.PI - Rotation;
        if (FlipY) return -Rotation;
        return Rotation;
    }

    public bool Equals(Transform2D other) => this.Position == other.Position && this.Scale == other.Scale;
    public static bool operator ==(in Transform2D t1, in Transform2D t2) => t1.Position == t2.Position && t1.Scale == t2.Scale;
    public static bool operator !=(in Transform2D t1, in Transform2D t2) => !(t1 == t2);
}