using Microsoft.CodeAnalysis;
using NForza.Cyrus.Templating;
using System.Collections.Generic;

namespace NForza.Cyrus.Generators;

public abstract class CyrusGeneratorBase
{
    public abstract void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext);

    public string GetPartialModelClass(string assemblyName, string subNamespace, string propertyName, string propertyType, IEnumerable<string> propertyValues, LiquidEngine liquidEngine)
    {
        var model = new
        {
            AssemblyName = assemblyName + "." + subNamespace,
            PropertyName = propertyName,
            PropertyType = propertyType,
            Properties = string.Join(",", propertyValues)
        };
        var source = liquidEngine.Render(model, "CyrusModel");
        return source;
    }
}