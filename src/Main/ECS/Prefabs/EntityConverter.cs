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

        MonodGame.entitySerializer.ReadEntities(
            entities,
            new MemoryStream(byteArray));

        if (entities.Count == 0)
        {
            Guard.JsonException("Entity doesn't contain any entity data.");
            return default;
        }

        Entity result = MonodGame.entityConverter.DataEntityToEntity(
            entities[0],
            MonodGame.PrefabsStore,
            out string error);

        if (!string.IsNullOrEmpty(error))
            Guard.JsonException($"Failed to load entity json: {error}");

        return result;

    }

    public override void Write(Utf8JsonWriter writer, Entity entity, JsonSerializerOptions options)
    {
        writer.WriteRawValue(MonodGame.entitySerializer.WriteEntity(entity));
    }
}