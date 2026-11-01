using OneOf;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Monod.Utils.General;

/// <summary>
/// Method utilities for json parsing. See <see cref="Json"/> for json option presets.
/// </summary>
public static class JsonUtil
{
    public static bool TryDeserializeArrayOrSingle<T>(Stream utf8Json, JsonSerializerOptions? options, out OneOf<T, T[]>? result, [NotNullWhen(false)] out Exception? exception)
    {
        result = null;

        if (utf8Json?.CanRead != true)
        {
            exception = new Exception("Stream doesn't support reading.");
            return false;
        }

        try
        {
            // Is there really no way to convert 'options' to JsonDocumentOptions? This is so terrible, I'll just leave DCommon here for now.
            using var document = JsonDocument.Parse(utf8Json, Json.DCommon);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                var array = document.RootElement.Deserialize<T[]>(options);

                if (array is null)
                {
                    exception = new JsonException("JSON array deserialized to null.");
                    return false;
                }

                result = array;
            }
            else
            {
                var value = document.RootElement.Deserialize<T>(options);

                if (value is null)
                {
                    exception = new JsonException("JSON object deserialized to null.");
                    return false;
                }

                result = value;
            }

            exception = null;
            return true;
        }
        catch (JsonException ex)
        {
            exception = ex;
            return false;
        }
        catch (NotSupportedException ex)
        {
            exception = ex;
            return false;
        }
    }

    /// <summary>
    /// Try to deserialize <typeparamref name="T"/> from the given <paramref name="utf8Json"/>.
    /// </summary>
    /// <typeparam name="T">Type of the object to deserialize.</typeparam>
    /// <param name="utf8Json">Stream, reading the json document.</param>
    /// <param name="options">Options to use when deserializing.</param>
    /// <param name="result">Deserialized result on sucess, default(T) on failure.</param>
    /// <param name="exception">Failure reason, or <see langword="null"/> on success.</param>
    /// <returns>Whether deserialization was successfull.</returns>
    public static bool TryDeserialize<T>(Stream utf8Json, JsonSerializerOptions? options, out T? result, [NotNullWhen(false)] out Exception? exception)
    {
        if (utf8Json?.CanRead != true)
        {
            result = default;
            exception = new Exception("Stream doesn't support reading.");
            return false;
        }

        try
        {
            result = JsonSerializer.Deserialize<T>(utf8Json, options);
            exception = null;
            return true;
        }
        catch (JsonException ex)
        {
            result = default;
            exception = ex;
            return false;
        }
        catch (NotSupportedException ex)
        {
            result = default;
            exception = ex;
            return false;
        }
    }
}
