using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators;

public abstract class CyrusGeneratorBase
{
    public abstract void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext);
}