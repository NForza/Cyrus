using System.Collections.Generic;

namespace NForza.Cyrus.Generators.Config;

public class GenerationConfig
{
    public string[] Contracts { get; set; } = ["Contracts"];
    public string EventBus { get; set; } = "Local";
    public List<GenerationTarget> GenerationTarget { get; set; } = [];
}