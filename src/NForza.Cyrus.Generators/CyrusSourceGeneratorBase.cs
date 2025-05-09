using System.Collections.Generic;
#if DEBUG_ANALYZER
using System.Diagnostics;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators;

public abstract class CyrusSourceGeneratorBase : IncrementalGeneratorBase
{
    protected IncrementalValueProvider<GenerationConfig> ConfigProvider(IncrementalGeneratorInitializationContext context)
    {
        var configProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.HasBaseType("CyrusConfig"),
                transform: (context, _) => ((ClassDeclarationSyntax)context.Node).GetConfigFromClass())
            .Collect()
            .Select((cfgs, _) => cfgs.FirstOrDefault() ?? CreateDefaultGenerationConfig());
        return configProvider;
    }

    private GenerationConfig CreateDefaultGenerationConfig()
    {
        return 
            new GenerationConfig 
            { 
                EventBus = EventBusType.Local,
                GenerationTarget = [GenerationTarget.Domain, GenerationTarget.WebApi, GenerationTarget.Contracts]
            };
    }

    public virtual void Initialize(IncrementalGeneratorInitializationContext context) { }

    private static LiquidEngine? liquidEngine = null;
    protected LiquidEngine LiquidEngine
    {
        get
        {
            liquidEngine ??= new LiquidEngine(Assembly.GetExecutingAssembly());
            return liquidEngine;
        }
    }
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
}
