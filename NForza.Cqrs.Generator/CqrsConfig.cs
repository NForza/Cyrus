using System.Collections.Generic;
using System.Linq;

namespace NForza.Cqrs.Generator;

public class CqrsConfig
{
    private Dictionary<string, List<string>> config;

    public CqrsConfig(Dictionary<string, List<string>> config)
    {
        var contracts = config["contracts"];
        if (contracts != null)
        {
            Contracts = contracts.ToArray();
        }
        var suffix = config["suffix"];
        if (suffix != null)
        {
            Commands.Suffix = suffix.First();
        }
        var handlerName = config["handlerName"];
        if (handlerName != null)
        {
            Commands.HandlerName= handlerName.First();
        }
    }

    public string[] Contracts { get; set; } = ["Contracts"];
    public CommandConfig Commands { get; set; }  = new();
}