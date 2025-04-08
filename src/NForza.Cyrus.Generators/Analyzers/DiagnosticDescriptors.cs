using Microsoft.CodeAnalysis;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MissingCommandAttribute = new DiagnosticDescriptor(
        id: "CYRUS001",
        title: "Missing [Command] attribute",
        messageFormat: "Command '{0}' must be marked with [Command]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Commands passed to a method marked with [CommandHandler] must be explicitly marked with the [Command] attribute."
    );

    public static readonly DiagnosticDescriptor TooManyArgumentsForCommandHandler = new DiagnosticDescriptor(
        id: "CYRUS002",
        title: "Too many arguments for CommandHandler",
        messageFormat: "CommandHandler '{0}' should have 1 argument",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command handers must have 1 parameter which is a [Command] object."
    );

    public static readonly DiagnosticDescriptor CommandHandlerShouldHaveACommandParameter = new DiagnosticDescriptor(
        id: "CYRUS003",
        title: "CommandHandler",
        messageFormat: "CommandHandler '{0}' should have an argument which is a [Command] object",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command handers must have 1 parameter which is a [Command] object."
    );


}
