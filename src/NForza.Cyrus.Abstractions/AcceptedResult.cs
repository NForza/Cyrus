namespace NForza.Cyrus.Abstractions;

public class AcceptedResult(string? location = null, object? value = null)
{
    public string? Location { get; } = location;
    public object? Value { get; } = value;
}
