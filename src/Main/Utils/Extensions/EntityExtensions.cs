using Monod.ECS.DefaultComponents;

namespace Monod.Utils.Extensions;

/// <summary>
/// Extensions for the <see cref="Entity"/>.
/// </summary>
public static class EntityExtensions
{
    public static Vector2 GetWorldPos(this Entity entity)
    {
        Vector2 pos = entity.GetLocalPos();
        Entity parent = entity.Parent;
        while (!parent.IsNull)
        {
            Vector2 parentPos = parent.GetLocalPos(out float rotation);
            pos.Rotate(rotation);
            pos += parentPos;
            parent = parent.Parent;
        }
        return pos;
    }

    public static Vector2 GetWorldPos(this Entity entity, out float rotation)
    {
        Vector2 pos = entity.GetLocalPos(out rotation);
        Entity parent = entity.Parent;
        while (!parent.IsNull)
        {
            Vector2 parentPos = parent.GetLocalPos(out float parentRot);
            pos.Rotate(parentRot);
            rotation += parentRot;
            pos += parentPos;
            parent = parent.Parent;
        }
        return pos;
    }

    public static Vector2 GetLocalPos(this Entity entity)
    {
        if (!entity.HasComponent<Transform2D>()) return Vector2.Zero;
        else return entity.GetComponent<Transform2D>().Position;
    }

    public static Vector2 GetLocalPos(this Entity entity, out float rotation)
    {
        if (!entity.HasComponent<Transform2D>())
        {
            rotation = 0;
            return Vector2.Zero;
        }
        else
        {
            ref Transform2D transform = ref entity.GetComponent<Transform2D>();
            rotation = transform.GetFlippedRotation();
            return transform.Position;
        }
    }
}
