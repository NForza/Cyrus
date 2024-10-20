using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace NForza.TypedIds;

public class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

    public override bool CanConvert(Type typeToConvert)
    {
        return TypedIdHelper.IsStronglyTypedId(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Cache.GetOrAdd(typeToConvert, CreateConverter);
    }

    private static JsonConverter CreateConverter(Type typeToConvert)
    {
        if (!TypedIdHelper.IsStronglyTypedId(typeToConvert, out var valueType))
            throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");

        var type = typeof(TypedIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
        return (JsonConverter)Activator.CreateInstance(type);
    }
}
