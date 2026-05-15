using Microsoft.Xna.Framework;
using Monod.PhysicsModule.Collisions.Interfaces;
using MonoGame.Extended;

namespace Monod.PhysicsModule.Collisions.Hitboxes;

public class RectangleHitbox : IPointCollidable
{
    public RectangleF Rect;

    public RectangleHitbox(Rectangle rect)
    {
        Rect = rect;
    }

    public bool IntersectsWithPoint(Vector2 point)
    {
        return point.X >= Rect.X && point.Y >= Rect.Y && point.X <= Rect.X + Rect.Width && point.Y <= Rect.Y + Rect.Height;
    }

    public float UntilCollisionWithPoint(Vector2 point, Vector2 speed)
    {
        throw new NotImplementedException("Didn't need this yet");
    }
}
