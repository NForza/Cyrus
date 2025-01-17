using NForza.Cyrus.TypedIds.Model;

namespace NForza.Cyrus.TypedIds;

public interface ICyrusModel
{
    IEnumerable<string> Guids { get => []; }
    IEnumerable<string> Integers { get => []; }
    IEnumerable<string> Strings { get => []; }
    IEnumerable<ModelDefinition> Events { get => []; }
    IEnumerable<ModelDefinition> Commands { get => []; }
}
