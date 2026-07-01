using Monod.AssetsModule;
using System.Text.Json.Serialization;

namespace Monod.ECS.Prefabs;

[JsonConverter(typeof(PrefabConverter))]
public abstract class Prefab
{
    public abstract Entity Instantiate(EntityStore store);
}

// if the class is ever unsealed, implement a proper dispose pattern.
public sealed class EntityPrefab : Prefab, IDisposable
{
    public Entity Source;

    public EntityPrefab(Entity source)
    {
        Source = source;
    }

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
    public Entity Source;

    [JsonIgnore]
    public Prefab? BasePrefab;

    public ModificationPrefab(string basePrefabName, Entity source)
    {
        BasePrefabPath = basePrefabName;
        Source = source;
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

        EntityStore.MergeEntity(Source, entity);

        return entity;
    }

    public void Dispose()
    {
        Assets.OnReload -= LoadAssets;
        Source.DeleteEntity();
    }
}