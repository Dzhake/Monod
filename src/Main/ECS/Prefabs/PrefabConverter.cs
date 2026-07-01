using Friflo.Engine.ECS.Serialize;
using Monod.Shared.Exceptions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.ECS.Prefabs;

public sealed class PrefabConverter : JsonConverter<Prefab>
{
    public override Prefab Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("$type", out JsonElement typeElement))
            throw new JsonException("Prefab doesn't contain '$type' property.");

        string? type = typeElement.GetString();

        switch (type)
        {
            case nameof(EntityPrefab):
                return ParseEntityPrefab(root);
            case nameof(ModificationPrefab):
                return ParseModificationPrefab(options, root);
            default:
                Guard.JsonException($"Unknown prefab type: {type}");
                return null;
        }
    }


    private static EntityPrefab ParseEntityPrefab(JsonElement root)
    {
        if (!root.TryGetProperty(nameof(EntityPrefab.Source), out var source))
        {
            Guard.JsonException($"Prefab of type {nameof(EntityPrefab)} doesn't contain 'Source' property.");
            return null!;
        }
        string json = $"[{root.GetProperty("Source").GetRawText()}]";
        byte[] byteArray = Encoding.UTF8.GetBytes(json);

        List<DataEntity> entities = [];

        MonodGame.entitySerializer.ReadEntities(
            entities,
            new MemoryStream(byteArray));

        if (entities.Count == 0)
        {
            Guard.JsonException($"Prefab of type {nameof(EntityPrefab)} doesn't contain any entity data.");
            return null!;
        }

        Entity result = MonodGame.entityConverter.DataEntityToEntity(
            entities[0],
            MonodGame.PrefabsStore,
            out string error);

        if (!string.IsNullOrEmpty(error))
            Log.Error("Failed to load prefab: {Error}", error);

        return new EntityPrefab(result);
    }

    private static ModificationPrefab ParseModificationPrefab(JsonSerializerOptions options, JsonElement root)
    {
        return JsonSerializer.Deserialize<ModificationPrefab>(root.GetRawText(), options);
    }

    public override void Write(Utf8JsonWriter writer, Prefab value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("TODO");
    }
}