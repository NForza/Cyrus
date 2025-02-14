using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class MethodDeclarationSyntaxExtensions
{
    public static bool HasQueryHandlerAttribute(this MethodDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "QueryHandler");
    }

    public static bool HasEventHandlerAttribute(this MethodDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "EventHandler");
    }

    public static bool HasCommandHandlerAttribute(this MethodDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "CommandHandler");
    }

    public static bool HasAttribute(this MethodDeclarationSyntax methodDeclaration, string attributeName)
    {
        return methodDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == attributeName);
    }
}