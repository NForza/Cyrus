using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Cqrs.Generator;

[Generator]
public class CqrsQueryHandlerGenerator : CyrusGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);;
        var configProvider = ConfigProvider(context);

        var incrementalValuesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (syntaxNode, _) => CouldBeQueryHandler(syntaxNode),
                transform: (context, _) => GetMethodSymbolFromContext(context));

        var allQueryHandlersProvider = incrementalValuesProvider.Combine(configProvider)
            .Where(x =>
            {
                var (methodNode, config) = x;
                return IsQueryHandler(methodNode, config.Queries.HandlerName, config.Queries.Suffix);
            })
            .Select((x, _) => x.Left!)
            .Collect();

        var combinedProvider = allQueryHandlersProvider.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, queryHandlersWithCompilation) =>
        {
            var (queryHandlers, compilation) = queryHandlersWithCompilation;
            if (queryHandlers.Any())
            {
                var sourceText = GenerateQueryProcessorExtensionMethods(queryHandlers, compilation);
                spc.AddSource($"QueryProcessor.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    private string GenerateQueryProcessorExtensionMethods(ImmutableArray<IMethodSymbol> handlers, Compilation compilation)
    {
        INamedTypeSymbol? taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (taskSymbol == null)
        {
            return string.Empty;
        }

        var queries = handlers.Select(h => new
        {
            Handler = h,
            QueryType = h.Parameters[0].Type.ToFullName(),
            ReturnType = (INamedTypeSymbol) h.ReturnType,
            IsAsync = h.ReturnType.OriginalDefinition.Equals(taskSymbol, SymbolEqualityComparer.Default)
        }).ToList();

        var model = new
        {
            Queries = queries.Select(q => new
            {
                ReturnTypeOriginal = q.ReturnType,
                ReturnType = q.IsAsync ? q.ReturnType.TypeArguments[0].ToFullName() : q.ReturnType.ToFullName(),
                q.QueryType,
                q.IsAsync
            }).ToList()
        };

        var resolvedSource = ScribanEngine.Render("QueryProcessorExtensions", model );

        return resolvedSource;
    }
}