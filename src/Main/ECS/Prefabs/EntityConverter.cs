using Friflo.Engine.ECS.Serialize;
using Monod.Shared.Exceptions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.ECS.Prefabs;

public sealed class EntityConverter : JsonConverter<Entity>
{
    public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        string json = $"[{root.GetRawText()}]";
        byte[] byteArray = Encoding.UTF8.GetBytes(json);

        var entities = new List<DataEntity>();

        new EntitySerializer().ReadEntities(entities, new MemoryStream(byteArray));

        if (entities.Count == 0)
        {
            Guard.JsonException("Entity doesn't contain any entity data.");
            return default;
        }

        DataEntity dataEntity = entities[0];
        Entity newEntity = MonodGame.PrefabsStore.CreateEntity();
        dataEntity.pid = newEntity.Pid;

        Entity result = new Friflo.Engine.ECS.Serialize.EntityConverter().DataEntityToEntity(dataEntity, MonodGame.PrefabsStore, out string error);

        if (!string.IsNullOrEmpty(error))
            Guard.JsonException($"Failed to load entity json: {error}");

        return result;

    }

    public override void Write(Utf8JsonWriter writer, Entity entity, JsonSerializerOptions options)
    {
        writer.WriteRawValue(new EntitySerializer().WriteEntity(entity));
    }
}