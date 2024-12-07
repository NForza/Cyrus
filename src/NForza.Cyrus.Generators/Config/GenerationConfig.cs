using System.Collections.Generic;
using System.Linq;
using NForza.Generators;

namespace NForza.Cyrus.Cqrs.Generator.Config;

public class GenerationConfig 
{
    public string[] Contracts { get; set; } = ["Contracts"];
    public CommandConfig Commands { get; set; } = new();
    public QueryConfig Queries { get; set; } = new();
    public EventConfig Events { get; set; } = new();
    public string[] GenerationType { get; set; } = ["domain", "webapi", "contracts"];
}