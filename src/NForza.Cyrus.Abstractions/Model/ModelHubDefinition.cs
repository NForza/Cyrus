namespace NForza.Cyrus.Abstractions.Model;

public record ModelHubDefinition(string Name, string Path, IEnumerable<string> Commands, IEnumerable<ModelQueryDefinition> Queries, IEnumerable<string> Events, IEnumerable<ModelTypeDefinition> SupportTypes) : INamedModelType;
