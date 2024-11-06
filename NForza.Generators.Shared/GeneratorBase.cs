using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1035 // Do not use APIs banned for analyzers

namespace NForza.Generators;

public abstract class GeneratorBase : ISourceGenerator
{
    protected TemplateEngine TemplateEngine = new(Assembly.GetExecutingAssembly(), "Templates");
    protected T ParseConfigFile<T>(GeneratorExecutionContext context, string configFileName)
        where T : IYamlConfig<T>, new()
    {
        var additionalFile = context.AdditionalFiles
            .FirstOrDefault(file => Path.GetFileName(file.Path) == configFileName);
        string configContent = additionalFile?.GetText(context.CancellationToken)?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configContent))
        {
            return new T();
        }
        var config = YamlParser.ReadYaml(configContent);
        return new T().InitFrom(config);
    }

    public void DebugThisGenerator(bool debug)
    {
#if DEBUG_ANALYZER
        if (!Debugger.IsAttached && debug)
        {
            Debugger.Launch();
        }
#endif
    }

    protected IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var nestedType in GetAllTypes(nestedNamespace))
                {
                    yield return nestedType;
                }
            }
            else if (member is INamedTypeSymbol namedType)
            {
                yield return namedType;
            }
        }
    }

    public virtual void Initialize(GeneratorInitializationContext context) { }

    public abstract void Execute(GeneratorExecutionContext context);
}
