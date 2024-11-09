using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class GuidIdGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);
        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithGuidIdAttribute(syntaxNode),
                        transform: (context, _) => GetSemanticTargetForGeneration(context));

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(recordStructsWithAttribute, (spc, recordSymbols) =>
        {
            foreach (var recordSymbol in recordSymbols)
            {
                var sourceText = GenerateGuidId(recordSymbol);
                spc.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
        });
    }

    private INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var recordStruct = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        return model.GetDeclaredSymbol(recordStruct) as INamedTypeSymbol;
    } 

    private bool IsRecordWithGuidIdAttribute(SyntaxNode syntaxNode)
    {
        bool isRecordWithGuidId = syntaxNode is RecordDeclarationSyntax recordDeclaration &&
                       recordDeclaration.HasAttribute("GuidId");
        return isRecordWithGuidId;
    }

    private string GenerateGuidId(INamedTypeSymbol item)
    {
        var replacements = new Dictionary<string, string>
        {
            ["ItemName"] = item.Name,
            ["Namespace"] = item.ContainingNamespace.ToDisplayString(),
            ["Constructor"] = GenerateConstructor(item),
            ["CastOperators"] = GenerateCastOperatorsToUnderlyingType(item),
            ["Default"] = "Guid.Empty"
        };

        var source = TemplateEngine.ReplaceInResourceTemplate("GuidId.cs", replacements);

        return source;
    }

    private string GenerateCastOperatorsToUnderlyingType(INamedTypeSymbol item) =>
        @$"public static implicit operator {GetUnderlyingTypeOfTypedId(item)}({item.ToDisplayString()} typedId) => typedId.Value;
    public static explicit operator {item.ToDisplayString()}({GetUnderlyingTypeOfTypedId(item)} value) => new(value);";

    string GenerateConstructor(INamedTypeSymbol item) =>
        $@"public {item.Name}(): this(Guid.NewGuid()) {{}}";
}