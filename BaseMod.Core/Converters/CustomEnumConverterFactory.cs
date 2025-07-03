using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseMod.Core.Converters;
public class CustomEnumConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        var converterType = typeof(CustomEnumConverter<>).MakeGenericType(typeToConvert);
        JsonConverter? converter = (JsonConverter?)Activator.CreateInstance(converterType) 
            ?? throw new InvalidOperationException($"Can't create instance for '{converterType.FullName}'");

        return converter;
    }
}
