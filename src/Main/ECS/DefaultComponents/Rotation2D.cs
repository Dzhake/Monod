using Friflo.Engine.ECS;

// ReSharper disable once CheckNamespace
namespace Monod.ECS.DefaultComponents;

/// <summary>
/// Rotation encoded as a <a href="https://en.wikipedia.org/wiki/Quaternion">Quaternion</a>
/// described by the mathematician W.R. Hamilton.
/// </summary>
[ComponentKey("Rotation2D")]
[ComponentSymbol("R")]
public struct Rotation2D : IComponent, IEquatable<Rotation2D>
{
    public float Angle;

    public readonly override string ToString() => Angle.ToString();

    public Rotation2D(float angle)
    {
        Angle = angle;
    }

    public bool Equals(Rotation2D other) => Angle == other.Angle;
    public static bool operator ==(in Rotation2D r1, in Rotation2D r2) => r1.Angle == r2.Angle;
    public static bool operator !=(in Rotation2D r1, in Rotation2D r2) => r1.Angle != r2.Angle;

    public override int GetHashCode() => Angle.GetHashCode();
    public override bool Equals(object obj) => obj is Rotation2D rotation && rotation.Equals(this);
}