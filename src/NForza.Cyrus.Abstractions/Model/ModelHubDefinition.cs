namespace NForza.Cyrus.Abstractions.Model;

public record ModelHubDefinition(string Name, string Path, IEnumerable<string> Commands, IEnumerable<string> Events) : INamedModelType;
