using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class MethodDeclarationSyntaxExtensions
{
    public static bool HasAttribute(this MethodDeclarationSyntax methodDeclaration, string attributeName)
    {
        return methodDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == attributeName);
    }
}