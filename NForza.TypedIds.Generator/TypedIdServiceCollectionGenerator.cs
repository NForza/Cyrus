//using System.Linq;
//using System.Text;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Text;

//namespace NForza.TypedIds.Generator;

//[Generator]
//public class TypedIdServiceCollectionGenerator : TypedIdGeneratorBase, IIncrementalGenerator
//{
//    public void Initialize(IncrementalGeneratorInitializationContext context)
//    {
//        IncrementalValuesProvider<TypedIdDefinition?> incrementalValuesProvider = context.SyntaxProvider
//                    .CreateSyntaxProvider(
//                        predicate: (syntaxNode, _) => IsRecordStructWithStringIdAttribute(syntaxNode),
//                        transform: (context, _) => GetSemanticTargetForGeneration(context));

//        var recordStructsWithAttribute = incrementalValuesProvider
//            .Where(x => x is not null)
//            .Select((x, _) => x!.Value)
//            .Collect();

//        context.RegisterSourceOutput(recordStructsWithAttribute, (spc, recordStructs) =>
//        {
//            foreach (var recordStruct in recordStructs)
//            {
//                context.RegisterSourceOutput(recordStructsWithAttribute, (spc, recordStructs) =>
//                {
//                    foreach (var recordStruct in recordStructs)
//                    {
//                            var sourceText = GenerateCodeForRecordStruct(recordStruct);
//                            spc.AddSource($"{recordStruct.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
//                    }
//                });
//                var sourceText = GenerateCodeForRecordStruct(recordStruct);
//                spc.AddSource($"{recordStruct.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
//            }
//        });
//    }

//    private static bool IsRecordStructWithStringIdAttribute(SyntaxNode syntaxNode)
//    {
//        return syntaxNode is RecordDeclarationSyntax recordDecl &&
//               recordDecl is { ClassOrStructKeyword.Text: "struct" } &&
//               recordDecl.AttributeLists.Count > 0;
//    }

//    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
//    {
//        var recordStruct = (RecordDeclarationSyntax)context.Node;
//        var model = context.SemanticModel;

//        var hasStringIdAttribute = recordStruct.AttributeLists
//            .SelectMany(al => al.Attributes)
//            .Any(attr => model.GetSymbolInfo(attr).Symbol is IMethodSymbol methodSymbol &&
//                         methodSymbol.ContainingType.Name == "StringId");

//        if (!hasStringIdAttribute)
//            return null;

//        var symbol = model.GetDeclaredSymbol(recordStruct) as INamedTypeSymbol;
//        if (symbol == null)
//            return null;

//        return symbol;
//    }

//    private static string GenerateCodeForRecordStruct(TypedIdDefinition recordStruct)
//    {
//        var (name, ns) = recordStruct;

//        var sb = new StringBuilder();
//        if (!string.IsNullOrEmpty(ns))
//        {
//            sb.AppendLine($"namespace {ns};");
//            sb.AppendLine();
//        }

//        sb.AppendLine($"public partial struct {name}");
//        sb.AppendLine("{");
//        sb.AppendLine("    public string Id { get; }");
//        sb.AppendLine();
//        sb.AppendLine($"    public {name}(string id) => Id = id;");
//        sb.AppendLine("}");

//        return sb.ToString();
//    }

////    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "Environment.NewLine should not be a banned API.")]
////    private void GenerateServiceCollectionExtensionMethod(GeneratorExecutionContext context, IEnumerable<INamedTypeSymbol> typedIds)
////    {
////        var source = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "ServiceCollectionExtensions.cs");

////        var converters = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<JsonConverter, {t.Name}JsonConverter>();"));

////        var namespaces = string.Join(Environment.NewLine, typedIds.Select(t => t.ContainingNamespace.ToDisplayString()).Distinct().Select(ns => $"using {ns};"));

////        var types = string.Join(",", typedIds.Select(t => $"[typeof({t.ToDisplayString()})] = typeof({GetUnderlyingTypeOfTypedId(t)})"));
////        var registrations = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<{t.ToDisplayString()}>();"));

////        source = source
////            .Replace("% AllTypes %", types)
////            .Replace("% AllTypedIdRegistrations %", registrations)
////            .Replace("% Namespaces %", namespaces)
////            .Replace("% AddJsonConverters %", converters);
////        context.AddSource($"ServiceCollectionExtensions.g.cs", source);
////    }
//}
