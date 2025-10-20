using System.Collections.Generic;

namespace NForza.Cyrus.Abstractions.Model;

public class CyrusMetadata: ICyrusModel
{
    public IEnumerable<string> Guids { get; set; } = [];
    public IEnumerable<string> Integers { get; set; } = [];
    public IEnumerable<string> Doubles { get; set; } = [];
    public IEnumerable<string> Strings { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Models { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Events { get; set; } = [];
    public IEnumerable<ModelTypeDefinition> Commands { get; set; } = [];
    public IEnumerable<ModelQueryDefinition> Queries { get; set; } = [];
    public IEnumerable<ModelEndpointDefinition> Endpoints { get; set; } = [];
    public IEnumerable<ModelHubDefinition> Hubs { get; set; } = [];
}
