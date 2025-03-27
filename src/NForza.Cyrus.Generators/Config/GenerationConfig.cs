using System.Collections.Generic;

namespace NForza.Cyrus.Generators.Config;

public class GenerationConfig
{
    public string[] Contracts { get; set; } = ["Contracts"];
    public EventBusType EventBus { get; set; } = EventBusType.Local;
    public List<GenerationTarget> GenerationTarget { get; set; } = [];
}