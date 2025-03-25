using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Templating;
using System.Collections.Generic;
using System.Reflection;

namespace NForza.Cyrus.Generators;

public abstract class CyrusGeneratorBase<T>
{
    public abstract IncrementalValueProvider<T> GetProvider(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<GenerationConfig> configProvider);
    public abstract void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine);

    public string GetPartialModelClass(string assemblyName, string subNamespace, string propertyName, string propertyType, IEnumerable<string> propertyValues)
    {
        var model = new
        {
            AssemblyName = assemblyName + "." + subNamespace,
            PropertyName = propertyName,
            PropertyType = propertyType,
            Properties = string.Join(",", propertyValues)
        };
        var source = LiquidEngine.Render(model, "CyrusModel");
        return source;
    }

    private static LiquidEngine? liquidEngine = null;
    protected LiquidEngine LiquidEngine
    {
        get
        {
            liquidEngine ??= new LiquidEngine(Assembly.GetExecutingAssembly());
            return liquidEngine;
        }
    }


}