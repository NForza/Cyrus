namespace NForza.Cqrs
{
    public class Configuration
    {
        public ContractsConfig Contracts { get; set; } = new();
        public CommandsConfig Commands { get; set; } = new();
    }

    public class ContractsConfig
    {
        public string ProjectSuffix { get; set; } = "Contracts";
    }

    public class CommandsConfig
    {
        public string CommandSuffix { get; set; } = "Command";
        public string HandlerMethodName { get; set; } = "Execute";
    }
}