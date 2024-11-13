//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.CodeAnalysis;
//using NForza.Generators;

//#pragma warning disable RS1035 // Do not use banned APIs for analyzers

//namespace NForza.Cqrs.Generator;

//[Generator]
//public class CqrsQueryHandlerGenerator : CqrsSourceGenerator, ISourceGenerator
//{    
//    public override void Execute(GeneratorExecutionContext context)
//    {
//        DebugThisGenerator(false);

//        base.Execute(context);

//        IEnumerable<string> contractSuffix = Configuration.Contracts;
//        var querySuffix = Configuration.Queries.Suffix;
//        var methodHandlerName = Configuration.Queries.HandlerName;

//        var queries = GetAllQueries(context.Compilation, contractSuffix, querySuffix).ToList();
//        var handlers = GetAllQueryHandlers(context, methodHandlerName, queries);

//        GenerateQueryProcessorExtensionMethods(context, handlers);
//    }

//    private void GenerateQueryProcessorExtensionMethods(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
//    {
//        StringBuilder source = new();
//        foreach (var handler in handlers)
//        {
//            var methodSymbol = handler;
//            var parameterType = methodSymbol.Parameters[0].Type;
//            var returnType = methodSymbol.ReturnType;
//            source.Append($@"
//    public static {returnType} Query(this IQueryProcessor queryProcessor, {parameterType} command, CancellationToken cancellationToken = default) 
//        => queryProcessor.QueryInternal<{parameterType}, {returnType}>(command, cancellationToken);");
//        }

//        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("QueryProcessorExtensions.cs", new Dictionary<string, string>
//        {
//            ["QueryMethods"] = source.ToString()
//        });

//        context.AddSource($"QueryProcessor.g.cs", resolvedSource.ToString());
//    }
//}