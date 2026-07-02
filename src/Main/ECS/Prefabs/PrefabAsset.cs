using Monod.AssetsModule;
using System.Text.Json.Serialization;

namespace Monod.ECS.Prefabs;

[JsonPolymorphic]
[JsonDerivedType(typeof(EntityPrefab), nameof(EntityPrefab))]
[JsonDerivedType(typeof(ModificationPrefab), nameof(ModificationPrefab))]
public abstract class PrefabAsset
{
    public abstract Entity Instantiate(EntityStore store);
}

// if the class is ever unsealed, implement a proper dispose pattern.
public sealed class EntityPrefab : PrefabAsset, IDisposable
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
public sealed class ModificationPrefab : PrefabAsset, IDisposable
{
    public Entity Source;

    public string BasePrefabPath;

    [JsonIgnore]
    public PrefabAsset? BasePrefab;

    public ModificationPrefab(string basePrefabName, Entity source)
    {
        BasePrefabPath = basePrefabName;
        Source = source;
        Assets.OnReload += LoadAssets;
        LoadAssets();
    }

    public void LoadAssets()
    {
        BasePrefab = Assets.GetOrDefault<PrefabAsset>(BasePrefabPath);
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