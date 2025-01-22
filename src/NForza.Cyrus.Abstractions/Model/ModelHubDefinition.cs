namespace NForza.Cyrus.Abstractions.Model;

public record ModelHubDefinition(string Name, IEnumerable<string> Commands): INamedModelType;
