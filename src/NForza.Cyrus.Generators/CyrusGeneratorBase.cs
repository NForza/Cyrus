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

public abstract class CyrusGeneratorBase : IncrementalGeneratorBase
{
    private static LiquidEngine? liquidEngine = null;  
    protected LiquidEngine LiquidEngine 
    { 
        get
        {
            liquidEngine ??= new LiquidEngine(Assembly.GetExecutingAssembly());
            return liquidEngine;
        }
    } 

    protected IncrementalValueProvider<GenerationConfig> ConfigProvider(IncrementalGeneratorInitializationContext context)
    {
        var configProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.Identifier.Text == "CyrusConfiguration",
                transform: (context, _) => ((ClassDeclarationSyntax)context.Node).GetConfigFromClass())
            .Collect()
            .Select((cfgs, _) => cfgs.First());
        return configProvider;
    }

    public virtual void Initialize(IncrementalGeneratorInitializationContext context) { }

    protected string GetTypeName(TypeSyntax? typeSyntax)
    {
        return typeSyntax switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
            QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.Text,
            _ => typeSyntax?.ToString() ?? "",
        };
    }

    public string GetPartialModelClass(string assemblyName, string propertyName, string propertyType, IEnumerable<string> propertyValues)
    {
        var model = new 
        {
            AssemblyName = assemblyName,
            PropertyName = propertyName,
            PropertyType = propertyType,
            Properties = string.Join(",", propertyValues)
        };
        var source = LiquidEngine.Render(model, "CyrusModel");
        return source;
    }
}
