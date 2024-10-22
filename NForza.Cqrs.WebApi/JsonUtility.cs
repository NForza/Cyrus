#nullable enable
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NForza.Cqrs.WebApi;

/// <summary>
/// Utility class for System.Text.Json
/// </summary>
internal static class JsonUtility
{
    public static void PopulateObject(Type type, object target, string jsonSource, JsonSerializerOptions options)
    {
        var json = JsonDocument.Parse(jsonSource).RootElement;
        foreach (var property in json.EnumerateObject())
            OverwriteProperty(property);

        void OverwriteProperty(JsonProperty updatedProperty)
        {
            var propertyInfo = type.GetProperty(updatedProperty.Name,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (!(propertyInfo?.SetMethod?.IsPublic).GetValueOrDefault())
                return;

            if (propertyInfo?.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                return;

            // If the property has a Converter attribute, we use it
            var converter = GetJsonConverter(propertyInfo!);
            if (converter != null)
            {
                var serializerOptions = new JsonSerializerOptions(options);
                serializerOptions.Converters.Add(converter);
                var parsedValue = JsonSerializer.Deserialize(updatedProperty.Value.GetRawText(),
                    propertyInfo!.PropertyType, serializerOptions);
                propertyInfo.SetValue(target, parsedValue);
            }
            else
            {
                var parsedValue = JsonSerializer.Deserialize(updatedProperty.Value.GetRawText(),
                    propertyInfo?.PropertyType ?? throw new InvalidOperationException("Can't determine property type for " + updatedProperty.Name), options);
                propertyInfo.SetValue(target, parsedValue);
            }
        }
    }

    private static JsonConverter? GetJsonConverter(PropertyInfo propertyInfo)
    {
        var attribute = propertyInfo.GetCustomAttribute<JsonConverterAttribute>();
        if (attribute == null) return null;

        if (attribute.ConverterType == null)
            return attribute.CreateConverter(propertyInfo.PropertyType);

        var ctor = attribute.ConverterType.GetConstructor(Type.EmptyTypes);
        if (typeof(JsonConverter).IsAssignableFrom(attribute.ConverterType) &&
            (ctor?.IsPublic).GetValueOrDefault())
            return (JsonConverter)Activator.CreateInstance(attribute.ConverterType)!;

        return null;
    }
}