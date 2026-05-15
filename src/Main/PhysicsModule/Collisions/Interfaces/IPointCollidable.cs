using Microsoft.Xna.Framework;

namespace Monod.PhysicsModule.Collisions.Interfaces;

public interface IPointCollidable
{
    public bool IntersectsWithPoint(Vector2 point);
    public float UntilCollisionWithPoint(Vector2 point, Vector2 speed);
}