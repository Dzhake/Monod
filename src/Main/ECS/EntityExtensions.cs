namespace Monod.ECS;

public static class EntityExtensions
{
    public static void RemoveComponentByType(this ref Entity entity, Type componentType)
    {
        EntitySchema schema = EntityStore.GetEntitySchema();
        var componentInternalType = schema.ComponentTypeByType[componentType];
        EntityUtils.RemoveEntityComponent(entity, componentInternalType);
    }

    public static void AddOrChangeComponent(this ref Entity entity, IComponent component)
    {
        EntitySchema schema = EntityStore.GetEntitySchema();
        var componentType = schema.ComponentTypeByType[component.GetType()];
        EntityUtils.AddEntityComponent(entity, componentType);
        EntityUtils.AddEntityComponentValue(entity, componentType, component);
    }
}
