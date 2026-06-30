using Monod.AssetsModule;
using System.Text.Json.Serialization;

namespace Monod.ECS.Prefabs;

public abstract class Prefab
{
    public abstract Entity Instantiate(EntityStore store);
}

// if the class is ever unsealed, implement a proper dispose pattern.
public sealed class EntityPrefab : Prefab, IDisposable
{
    public Entity Source;

    public override Entity Instantiate(EntityStore store)
    {
        Entity target = store.CreateEntity();
        Source.CopyEntity(target);
        return target;
    }

    public void Dispose()
    {
        Source.DeleteEntity();
    }
}

// if the class is ever unsealed, implement a proper dispose pattern.
public sealed class ModificationPrefab : Prefab, IDisposable
{
    public string BasePrefabPath;
    public Entity? Source;
    public Type[]? RemoveComponents;

    [JsonIgnore]
    public Prefab? BasePrefab;

    public ModificationPrefab(string basePrefabName, Entity? source = null, Type[]? removeComponents = null)
    {
        BasePrefabPath = basePrefabName;
        Source = source;
        RemoveComponents = removeComponents;
        Assets.OnReload += LoadAssets;
        LoadAssets();
    }

    public void LoadAssets()
    {
        BasePrefab = Assets.GetOrDefault<Prefab>(BasePrefabPath);
    }

    public override Entity Instantiate(EntityStore store)
    {
        Entity entity = BasePrefab?.Instantiate(store) ?? store.CreateEntity();

        if (RemoveComponents is not null)
        {
            foreach (var componentTypeToRemove in RemoveComponents)
                entity.RemoveComponentByType(componentTypeToRemove);
        }

        if (Source is not null)
            EntityStore.MergeEntity(Source.Value, entity);

        return entity;
    }

    public void Dispose()
    {
        Assets.OnReload -= LoadAssets;
        Source?.DeleteEntity();
    }
}