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

    public static Vector2 GetParentPos(this Entity entity, out float parentRotation)
    {
        Vector2 pos = Vector2.Zero;
        parentRotation = 0;
        Entity parent = entity.Parent;
        while (!parent.IsNull)
        {
            Vector2 parentPos = parent.GetLocalPos(out float parentRot);
            pos.Rotate(parentRot);
            parentRotation += parentRot;
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



    /// <summary>
    /// Get the component if entity has it, or get a reference to component in the <see cref="CommandBuffer"/>'s cache (warning: don't use the reference after <see cref="CommandBuffer.Playback"/> call!)
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="data">Data of the entity.</param>
    /// <param name="cb">Command buffer that handles adding component if absent.</param>
    /// <returns>See summary.</returns>
    public static ref T Ensure<T>(this EntityData data, CommandBuffer cb) where T : struct, IComponent
    {
        if (data.Has<T>())
        {
            return ref data.Get<T>();
        }
        else
        {
            return ref cb.AddComponent(data.Id, new T());
        }
    }

    /// <summary>
    /// Same as <see cref="Ensure{T}(EntityData, CommandBuffer)"/>, but also returns whether the component was added or already was there. Useful for running the component some default values.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="data">Data of the entity.</param>
    /// <param name="cb">Command buffer that handles adding component if absent.</param>
    /// <param name="added">Whether the component was added of already was there.</param>
    /// <returns>See summary.</returns>
    public static ref T Ensure<T>(this EntityData data, CommandBuffer cb, out bool added) where T : struct, IComponent
    {
        if (data.Has<T>())
        {
            added = false;
            return ref data.Get<T>();
        }
        else
        {
            added = true;
            return ref cb.AddComponent(data.Id, new T());
        }
    }



    /// <summary>
    /// Delegate for the <see cref="ApplyRecursive{T}(in Entity, ApplyRecursiveAction{T})"/>. Meant to be 'applied' to the <paramref name="entity"/> if it has the <typeparamref name="T"/> component.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="entity">Entity with the component.</param>
    /// <param name="component">The component.</param>
    public delegate void ApplyRecursiveAction<T>(in Entity entity, ref T component) where T : struct, IComponent;

    /// <summary>
    /// Call the <paramref name="callback"/> for the <paramref name="entity"/> and each child entity of the <paramref name="entity"/>, if it has the <typeparamref name="T"/> component.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="entity">Entity. The callback is applied to the entity, and all of its child entity, recursively.</param>
    /// <param name="callback">Callback to call.</param>
    public static void ApplyRecursive<T>(in this Entity entity, ApplyRecursiveAction<T> callback) where T : struct, IComponent
    {
        if (entity.HasComponent<T>()) callback(in entity, ref entity.GetComponent<T>());
        foreach (Entity child in entity.ChildEntities)
            ApplyRecursive(child, callback);
    }

    /// <summary>
    /// Set <see cref="Transform2D.PhysicsDirty"/> for the <paramref name="entity"/> and all of its child entities.
    /// </summary>
    public static void SetTransformDirtyRecursive(this Entity entity) => entity.ApplyRecursive<Transform2D>((in entity, ref transform) => transform.PhysicsDirty = true);


    /// <summary>
    /// Get the first entity with the <typeparamref name="T"/> component, starting with <paramref name="entity"/> and going up the hierarchy.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    /// <param name="entity"></param>
    /// <returns>Entity with the <typeparamref name="T"/> component.</returns>
    public static Entity GetFirstEntityWithComp<T>(in this Entity entity) where T : struct, IComponent
    {
        Entity bodyEntity = entity;
        while (!bodyEntity.IsNull && !bodyEntity.HasComponent<T>())
            bodyEntity = bodyEntity.Parent;
        return bodyEntity;
    }
}
