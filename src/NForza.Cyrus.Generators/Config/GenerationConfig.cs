using System.Collections.Generic;

namespace NForza.Cyrus.Generators.Config;

public class GenerationConfig
{
    public List<GenerationTarget> GenerationTarget { get; set; } = [];
    public string PersistenceContextType { get; set; } = string.Empty;
}