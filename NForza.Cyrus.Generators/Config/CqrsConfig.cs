using System.Collections.Generic;
using System.Linq;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator.Config;

public class CqrsConfig : IYamlConfig<CqrsConfig>
{
    public CqrsConfig InitFrom(Dictionary<string, List<string>> config)
    {
        var type = config["type"];
        if (type != null)
        {
            GenerationType = type.First();
        }
        if (config.ContainsKey("contracts"))
        {
            Contracts = [.. config["contracts"]];
        }

        if (config.ContainsKey("suffix"))
        {
            Commands.Suffix = config["suffix"].First();
        }
        if (config.ContainsKey("handlerName"))
        {
            Commands.HandlerName = config["handlerName"].First();
        }
        if (config.ContainsKey("eventBus"))
        {
            EventBus = config["eventBus"].First();
        }
        return this;
    }

    public string[] Contracts { get; set; } = ["Contracts"];
    public CommandConfig Commands { get; set; } = new();
    public CommandConfig Queries { get; set; } = new() { HandlerName = "Query", Suffix = "Query" };
    public string EventBus { get; set; } = "Local";
    public string GenerationType { get; private set; } = "domain";
}