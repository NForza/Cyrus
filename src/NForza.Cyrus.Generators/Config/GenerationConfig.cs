using System.Collections.Generic;

namespace NForza.Cyrus.Generators.Config;

public class GenerationConfig
{
    public EventBusType EventBus { get; set; } = EventBusType.Local;
    public List<GenerationTarget> GenerationTarget { get; set; } = [];
    public PersistenceType? Persistence { get; set; } = null;
}