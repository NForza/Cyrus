using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Abstractions;

public interface ICyrusModel
{
    IEnumerable<string> Guids { get => []; }
    IEnumerable<string> Integers { get => []; }
    IEnumerable<string> Strings { get => []; }
    IEnumerable<ModelDefinition> Events { get => []; }
    IEnumerable<ModelDefinition> Commands { get => []; }
}
