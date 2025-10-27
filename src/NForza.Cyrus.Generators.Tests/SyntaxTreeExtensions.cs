
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Generators.Tests;

internal static class SyntaxTreeExtensions
{
    extension(IEnumerable<SyntaxTree> syntaxTrees)
    {
        public CyrusMetadata GetGeneratedModel()
        {
            var modelTree = syntaxTrees.FirstOrDefault(st => st.FilePath.Contains("cyrus-model.g.cs"));
            if (modelTree == null)
            {
                throw new InvalidOperationException("No generated model found in the provided syntax trees.");
            }
         
            var compressedModel = GetCompressedCyrusModel(modelTree);
            var modelJson = compressedModel.DecompressFromBase64();
            var model = System.Text.Json.JsonSerializer.Deserialize<CyrusMetadata>(modelJson, ModelSerializerOptions.Default);
            return model ?? throw new InvalidOperationException("Failed to deserialize the generated model.");
        }

        static string? GetCompressedCyrusModel(SyntaxTree tree)
        {
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var attr = root.AttributeLists
                .Where(al => al.Target?.Identifier.IsKind(SyntaxKind.AssemblyKeyword) == true) // [assembly: ...]
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(a => a.Name.ToString().EndsWith("AssemblyMetadata"));

            if (attr?.ArgumentList?.Arguments.Count >= 2 &&
                attr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax keyLit &&
                keyLit.Token.ValueText == "cyrus-model" &&
                attr.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax valLit)
            {
                return valLit.Token.ValueText; // <-- your "H4sIA..." string
            }
            return null;
        }
    }
}
