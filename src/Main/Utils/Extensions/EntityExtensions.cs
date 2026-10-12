using Monod.ECS.DefaultComponents;

namespace Monod.Utils.Extensions;

/// <summary>
/// Extensions for the <see cref="Entity"/>.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Get position of the entity accounting for the parent's transform.
    /// </summary>
    /// <param name="entity">Entity to get position of.</param>
    /// <returns>Position of the entity accounting for the parent's transform.</returns>
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

    /// <summary>
    /// Get position and rotation of the entity accounting for the parent's transform.
    /// </summary>
    /// <param name="entity">Entity to get position and rotation of.</param>
    /// <param name="rotation">Rotation of the entity, including its own rotation.</param>
    /// <returns>Position of the entity accounting for the parent's transform.</returns>
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

    /// <summary>
    /// Returns position of the entity from its <see cref="Transform2D"/>, or <see cref="Vector2.Zero"/> if the entity doesn't have <see cref="Transform2D"/> component.
    /// </summary>
    /// <param name="entity">Entity to get position of.</param>
    /// <returns>Position of the entity from its <see cref="Transform2D"/>, or <see cref="Vector2.Zero"/> if the entity doesn't have <see cref="Transform2D"/> component.</returns>
    public static Vector2 GetLocalPos(this Entity entity)
    {
        if (!entity.HasComponent<Transform2D>()) return Vector2.Zero;
        else return entity.GetComponent<Transform2D>().Position;
    }

    /// <summary>
    /// Returns position and rotation of the entity from its <see cref="Transform2D"/>, or <see cref="Vector2.Zero"/> and 0 if the entity doesn't have <see cref="Transform2D"/> component.
    /// </summary>
    /// <param name="entity">Entity to get position and rotation of.</param>
    /// <param name="rotation">Rotation of the entity in radians.</param>
    /// <returns>Position and rotation of the entity from its <see cref="Transform2D"/>, or <see cref="Vector2.Zero"/> and 0 if the entity doesn't have <see cref="Transform2D"/> component.</returns>
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
