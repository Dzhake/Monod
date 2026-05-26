using Microsoft.Xna.Framework;

namespace Monod.ECS.Tweening;

public class Vector2Lerper : ILerper<Vector2>
{
    public Vector2 Lerp(Vector2 from, Vector2 to, float time) => Vector2.Lerp(from, to, time);
}