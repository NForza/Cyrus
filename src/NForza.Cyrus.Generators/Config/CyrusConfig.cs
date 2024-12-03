using System.Collections.Generic;
using System.Linq;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator.Config;

public class CyrusConfig : IYamlConfig<CyrusConfig>
{
    public CyrusConfig InitFrom(Dictionary<string, List<string>> config)
    {
        var type = config["type"];
        if (type != null)
        {
            GenerationType = [.. config["type"]];
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
    public QueryConfig Queries { get; set; } = new();
    public string EventBus { get; set; } = "Local";
    public string[] GenerationType { get; set; } = ["domain", "webapi", "contracts"];
}