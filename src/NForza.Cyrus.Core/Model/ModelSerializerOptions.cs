using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

[JsonSerializable(typeof(ICyrusModel))]
[JsonSerializable(typeof(ModelHubDefinition))]
[JsonSerializable(typeof(ModelPropertyDefinition))]
[JsonSerializable(typeof(ModelQueryDefinition))]
[JsonSerializable(typeof(ModelTypeDefinition))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SerializationContext : JsonSerializerContext;

public static class ModelSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
            if (prop.PropertyType.IsAssignableTo(typeof(IEnumerable)))
            {
                prop.ShouldSerialize = static (_, child) => child is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
            }
        }
    }
}
