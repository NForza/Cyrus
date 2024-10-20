using System.Text.Json.Serialization;
using System.Text.Json;

namespace NForza.TypedIds;

public class TypedIdJsonConverter<TStronglyTypedId, TValue> : JsonConverter<TStronglyTypedId>
    where TStronglyTypedId : TypedId<TValue>
    where TValue : notnull
{
    public override TStronglyTypedId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        var value = JsonSerializer.Deserialize<TValue>(ref reader, options) ?? throw new InvalidOperationException($"Can't deserialize {typeToConvert}");
        var factory = TypedIdHelper.GetFactory<TValue>(typeToConvert);
        return (TStronglyTypedId)factory(value);
    }

    public override void Write(Utf8JsonWriter writer, TStronglyTypedId value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value.Value, options);
    }
}
