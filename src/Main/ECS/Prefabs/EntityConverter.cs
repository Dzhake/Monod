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

        string json = root.GetRawText();
        if (!json.StartsWith('[')) // entity serializer only accepts arrays, so if the prefab is an object, make it one object array.
            json = $"[{json}]";
        byte[] byteArray = Encoding.UTF8.GetBytes(json);

        var entities = new List<DataEntity>();

        var entitySerializer = new EntitySerializer();
        entitySerializer.ReadEntities(entities, new MemoryStream(byteArray));

        if (entities.Count == 0)
        {
            Guard.JsonException("Json doesn't contain any entity data.");
            return default;
        }

        DataEntity dataEntity = entities[0];
        if (dataEntity.pid == 0) //some default value. Assigning random pid is not the recommended approach, but it's definitly better than entities overlaping.
        {
            Entity newEntity = MonodGame.PrefabsStore.CreateEntity();
            dataEntity.pid = newEntity.Pid;
        }

        var entityConverter = new Friflo.Engine.ECS.Serialize.EntityConverter();
        Entity result = entityConverter.DataEntityToEntity(dataEntity, MonodGame.PrefabsStore, out string error);

        if (!string.IsNullOrEmpty(error))
            Guard.JsonException($"Failed to load entity json: {error}");

        return result;

    }

    public override void Write(Utf8JsonWriter writer, Entity entity, JsonSerializerOptions options)
    {
        var entitySerializer = new EntitySerializer();
        writer.WriteRawValue(entitySerializer.WriteEntity(entity));
    }
}