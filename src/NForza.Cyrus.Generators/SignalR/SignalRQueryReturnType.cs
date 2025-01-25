namespace NForza.Cyrus.Generators.SignalR;

public record SignalRQueryReturnType
{
    public SignalRQueryReturnType(string name, bool isCollection, bool isNullable)
    {
        Name = name;
        IsCollection = isCollection;
        IsNullable = isNullable;
    }

    public string Name { get; internal set; } = string.Empty;
    public bool IsCollection { get; set; }
    public bool IsNullable { get; set; }
}