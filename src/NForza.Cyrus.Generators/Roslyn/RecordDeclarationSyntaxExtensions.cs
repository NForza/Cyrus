using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class RecordDeclarationSyntaxExtensions
{
    public static bool HasAttribute(this RecordDeclarationSyntax recordDeclaration, string attributeName)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == attributeName);
    }
}