using System.Collections.Generic;

namespace NForza.Cyrus.Abstractions.Model;

public interface ICyrusModel
{
    IEnumerable<string> Guids { get; }
    IEnumerable<string> Integers { get; }
    IEnumerable<string> Strings { get; }
    IEnumerable<ModelTypeDefinition> Models { get; }
    IEnumerable<ModelTypeDefinition> Events { get; }
    IEnumerable<ModelTypeDefinition> Commands { get; }
    IEnumerable<ModelQueryDefinition> Queries { get; }
    IEnumerable<ModelEndpointDefinition> Endpoints { get; }
    IEnumerable<ModelHubDefinition> Hubs { get; }
}