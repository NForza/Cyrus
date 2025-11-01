using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators;

public abstract class CyrusGeneratorBase
{
    public abstract void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext);
}