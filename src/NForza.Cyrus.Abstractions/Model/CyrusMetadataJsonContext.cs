using System.Text.Json.Serialization;

namespace NForza.Cyrus.Abstractions.Model;

[JsonSerializable(typeof(ICyrusModel))]
[JsonSerializable(typeof(CyrusMetadata))]
[JsonSerializable(typeof(ModelPropertyDefinition))]
[JsonSerializable(typeof(ModelEndpointDefinition))]
[JsonSerializable(typeof(ModelTypeDefinition))]
[JsonSerializable(typeof(ModelQueryDefinition))]
[JsonSerializable(typeof(ModelHubDefinition))]
public partial class CyrusMetadataJsonContext : JsonSerializerContext
{
}
