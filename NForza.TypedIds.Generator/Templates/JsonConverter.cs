using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace % NamespaceName %;

public class % TypedIdName %JsonConverter : JsonConverter<% TypedIdName %>
{
    public override % TypedIdName % Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guid = reader.% GetMethodName %();  
        return new % TypedIdName %(guid);
    }

    public override void Write(Utf8JsonWriter writer, % TypedIdName % value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString());
    }
}
