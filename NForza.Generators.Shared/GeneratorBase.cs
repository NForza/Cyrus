using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NForza.Generators;

public abstract class GeneratorBase
{
    protected TemplateEngine TemplateEngine = new(Assembly.GetExecutingAssembly(), "Templates");
    protected IncrementalValueProvider<T> ParseConfigFile<T>(IncrementalGeneratorInitializationContext context, string configFileName)
        where T : IYamlConfig<T>, new()
    {
        var configFile = context.AdditionalTextsProvider
            .Where(file => Path.GetFileName(file.Path) == configFileName)
            .Select((file, token) =>
            {
                var content = file.GetText(token)?.ToString();
                var config = YamlParser.ReadYaml(content ?? "");
                return new T().InitFrom(config);
            })
            .Collect()
            .Select((declarations, _) => declarations.First());

        return configFile;
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

    public virtual void Initialize(IncrementalGeneratorInitializationContext context) { }

    protected string GetTypeName(TypeSyntax? typeSyntax)
    {
        switch (typeSyntax)
        {
            case IdentifierNameSyntax identifierName:
                return identifierName.Identifier.Text;

            case QualifiedNameSyntax qualifiedName:
                return qualifiedName.Right.Identifier.Text;

            case PredefinedTypeSyntax predefinedType:
                return predefinedType.Keyword.Text;

            default:
                return typeSyntax?.ToString() ?? ""; 
        }
    }
}
