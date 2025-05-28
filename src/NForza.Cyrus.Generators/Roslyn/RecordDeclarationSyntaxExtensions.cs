using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Cyrus.Generators.Roslyn;

public static class RecordDeclarationSyntaxExtensions
{
    public static bool HasAggregateRootAttribute(this RecordDeclarationSyntax recordDeclaration)
        => recordDeclaration.HasAttribute("AggregateRoot");

    public static bool HasQueryAttribute(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "Query");
    }

    public static bool HasEventAttribute(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "Event");
    }

    public static bool HasCommandAttribute(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "Command");
    }

    public static bool HasQueryHandlerAttribute(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "QueryHandler");
    }

    public static bool HasEventHandlerAttribute(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "EventHandler");
    }

    public static bool HasCommandHandlerAttribute(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == "CommandHandler");
    }

    public static bool HasAttribute(this RecordDeclarationSyntax recordDeclaration, string attributeName)
    {
        return recordDeclaration.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .Any(attr => attr.Name.ToString() == attributeName);
    }
}