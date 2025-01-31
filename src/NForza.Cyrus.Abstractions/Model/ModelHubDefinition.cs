using System.Text.Json.Serialization;

namespace NForza.Cyrus.Abstractions.Model;

public record ModelHubDefinition(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] string Name, 
    string Path,
    IEnumerable<string> Commands, 
    IEnumerable<ModelQueryDefinition> Queries, 
    IEnumerable<string> Events, IEnumerable<ModelTypeDefinition> SupportTypes) : INamedModelType;
