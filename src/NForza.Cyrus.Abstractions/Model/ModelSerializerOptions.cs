using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NForza.Cyrus.Abstractions.Model;

[JsonSerializable(typeof(ICyrusModel))]
[JsonSerializable(typeof(ModelHubDefinition))]
[JsonSerializable(typeof(ModelPropertyDefinition))]
[JsonSerializable(typeof(ModelQueryDefinition))]
[JsonSerializable(typeof(ModelEndpointDefinition))]
[JsonSerializable(typeof(ModelTypeDefinition))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SerializationContext : JsonSerializerContext;

public static class ModelSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        TypeInfoResolver = CyrusMetadataJsonContext.Default,
        TypeInfoResolverChain = { SerializationContext.Default.WithAddedModifier(ContractModifier_Collection) },
    };

    private static void ContractModifier_Collection(JsonTypeInfo contract)
    {
        if (contract.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }
        foreach (var prop in contract.Properties)
        {
            if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {
                prop.ShouldSerialize = static (_, child) => child is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
            }
        }
    }
}
