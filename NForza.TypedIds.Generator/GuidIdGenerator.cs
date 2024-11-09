//using System.Collections.Generic;
//using Microsoft.CodeAnalysis;
//using NForza.Generators;

//namespace NForza.TypedIds.Generator;

//[Generator]
//public class GuidIdGenerator : TypedIdGeneratorBase, ISourceGenerator
//{
//    public override void Execute(GeneratorExecutionContext context)
//    {
//        DebugThisGenerator(false);

//        var typedIds = GetAllTypedIds(context.Compilation, "GuidIdAttribute");
//        foreach (var item in typedIds)
//        {
//            GenerateGuidId(context, item);
//        }
//    }

//    private void GenerateGuidId(GeneratorExecutionContext context, INamedTypeSymbol item)
//    {
//        var replacements = new Dictionary<string, string>
//        {
//            ["ItemName"] = item.Name,
//            ["Namespace"] = item.ContainingNamespace.ToDisplayString(),
//            ["Constructor"] = GenerateConstructor(item),
//            ["CastOperators"] = GenerateCastOperatorsToUnderlyingType(item),
//            ["Default"] = "Guid.Empty"
//        };

//        var source = TemplateEngine.ReplaceInResourceTemplate("GuidId.cs", replacements);

//        context.AddSource($"{item.Name}.g.cs", source.ToString());
//    }

//    private string GenerateCastOperatorsToUnderlyingType(INamedTypeSymbol item) => 
//        @$"public static implicit operator {GetUnderlyingTypeOfTypedId(item)}({item.ToDisplayString()} typedId) => typedId.Value;
//        public static explicit operator {item.ToDisplayString()}({GetUnderlyingTypeOfTypedId(item)} value) => new(value);";

//    string GenerateConstructor(INamedTypeSymbol item) => 
//        $@"public {item.Name}(): this(Guid.NewGuid()) {{}}";
//}