using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseMod.Core.Converters;
public class CustomEnumConverter<T> : JsonConverter<T> 
    where T : struct, Enum {

    private readonly bool isFlags = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return default;

        if (isFlags) {
            T result = default;
            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
                if (Enum.TryParse<T>(part, ignoreCase: true, out var parsed)) {
                    result = (T)(object)(((int)(object)result) | ((int)(object)parsed));
                } else {
                    throw new JsonException($"Invalid flag value '{part}' for enum {typeof(T).Name}");
                }
            }
            return result;
        } else {
            if (Enum.TryParse<T>(raw, ignoreCase: true, out var result))
                return result;

            throw new JsonException($"Invalid enum value '{raw}' for {typeof(T).Name}");
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}
