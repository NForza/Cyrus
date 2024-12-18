namespace NForza.Cyrus.Cqrs.Generator.Config;

public class CommandConfig
{
    public string Suffix { get; set; } = "Command";
    public string HandlerName { get; set; } = "Execute";
}