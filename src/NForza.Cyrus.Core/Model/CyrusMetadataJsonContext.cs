using System.Text.Json.Serialization;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

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
