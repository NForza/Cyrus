namespace NForza.Cyrus.TypedIds;

public interface ICyrusModel
{
    IEnumerable<string> Guids { get => []; }
    IEnumerable<string> Integers { get => []; }
    IEnumerable<string> Strings { get => []; }
    IEnumerable<string> Events { get => []; }
    IEnumerable<string> Commands { get => []; }
}
