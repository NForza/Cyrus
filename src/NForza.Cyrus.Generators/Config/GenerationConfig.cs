using System.Collections.Generic;
using NForza.Cyrus.Generators.Config;

namespace NForza.Cyrus.Cqrs.Generator.Config;

public class GenerationConfig 
{
    public string[] Contracts { get; set; } = ["Contracts"];
    public CommandConfig Commands { get; set; } = new();
    public QueryConfig Queries { get; set; } = new();
    public EventConfig Events { get; set; } = new();
    public List<GenerationTarget> GenerationTarget { get; set; } = [];
}