using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Analyzers;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InternalCyrusError = new DiagnosticDescriptor(
        id: "CYRUSERROR001",
        title: "Internal Cyrus Error",
        messageFormat: "Internal Cyrus Error: {0}",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An internal error occurred in the Cyrus Source Generator."
    );

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
        messageFormat: "CommandHandler '{0}' should have 3 arguments max. (Command, Aggregate, CancellationToken).",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command handlers have a maximum of 3 arguments: a required Command, optionally an Aggregate that the command runs on and optionally a CancellationToken."
    );

    public static readonly DiagnosticDescriptor CommandHandlerShouldHaveACommandParameter = new DiagnosticDescriptor(
        id: "CYRUS003",
        title: "CommandHandler",
        messageFormat: "CommandHandler '{0}' should have an argument which is a [Command] object",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command handlers must have 1 parameter which is a [Command] object."
    );

    public static readonly DiagnosticDescriptor MissingQueryAttribute = new DiagnosticDescriptor(
        id: "CYRUS004",
        title: "Missing [Query] attribute",
        messageFormat: "Query '{0}' must be marked with [Query]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Querys passed to a method marked with [QueryHandler] must be explicitly marked with the [Query] attribute."
    );

    public static readonly DiagnosticDescriptor TooManyArgumentsForQueryHandler = new DiagnosticDescriptor(
        id: "CYRUS005",
        title: "Too many arguments for QueryHandler",
        messageFormat: "QueryHandler '{0}' should have 1 argument",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Query handlers must have 1 parameter which is a [Query] object."
    );

    public static readonly DiagnosticDescriptor QueryHandlerShouldHaveAQueryParameter = new DiagnosticDescriptor(
        id: "CYRUS006",
        title: "QueryHandler",
        messageFormat: "QueryHandler '{0}' should have an argument which is a [Query] object",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Query handlers must have 1 parameter which is a [Query] object."
    );

    public static readonly DiagnosticDescriptor MissingEventAttribute = new DiagnosticDescriptor(
        id: "CYRUS004",
        title: "Missing [Event] attribute",
        messageFormat: "Event '{0}' must be marked with [Event]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Events passed to a method marked with [EventHandler] must be explicitly marked with the [Event] attribute."
    );

    public static readonly DiagnosticDescriptor IncorrectNumberOfArgumentsForEventHandler = new DiagnosticDescriptor(
        id: "CYRUS005",
        title: "Too many arguments for EventHandler",
        messageFormat: "EventHandler '{0}' should have 1 argument",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Event handlers must have 1 parameter which is a [Event] object."
    );

    public static readonly DiagnosticDescriptor EventHandlerShouldHaveAEventParameter = new DiagnosticDescriptor(
        id: "CYRUS006",
        title: "EventHandler",
        messageFormat: "EventHandler '{0}' should have an argument which is a [Event] object",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Event handlers must have 1 parameter which is a [Event] object."
    );

    public static readonly DiagnosticDescriptor ProjectShouldReferenceCyrusMassTransit = new DiagnosticDescriptor(
        id: "CYRUS007",
        title: "MassTransit reference",
        messageFormat: "Project must reference NForza.Cyrus.MassTransit",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Project must reference NForza.Cyrus.MassTransit."
    );

    public static readonly DiagnosticDescriptor ValueTypeMustBeARecordStruct = new DiagnosticDescriptor(
        id: "CYRUS008",
        title: "ValueTypes",
        messageFormat: "ValueType {0} must be a record struct",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ValueTypes must be record structs."
    );

    public static readonly DiagnosticDescriptor IncorrectNumberOfArgumentsForValidator = new DiagnosticDescriptor(
        id: "CYRUS009",
        title: "Too many arguments for Validator",
        messageFormat: "Validator '{0}' should have 1 argument",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Validators must have 1 parameter which is a Command or Query object."
    );

    public static readonly DiagnosticDescriptor ValidatorsShouldReturnIEnumerableOfString = new DiagnosticDescriptor(
        id: "CYRUS009",
        title: "Incorrect return type for Validator",
        messageFormat: "Validator '{0}' should return IEnumerable<string>",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Validators should return IEnumerable<string>."
    );

    public static readonly DiagnosticDescriptor MissingHandler = new DiagnosticDescriptor(
        id: "CYRUS010",
        title: "Missing Handler",
        messageFormat: "Handler for '{0}' not found",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each command or query should have a handler."
    );

    public static readonly DiagnosticDescriptor UnableToDetermineReturnType = new DiagnosticDescriptor(
        id: "CYRUS011",
        title: "Unable to determine return type",
        messageFormat: "Unable to determine return type for {0}",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Unable to determine return type."
    );

    public static readonly DiagnosticDescriptor ResultTupleElementShouldBeCalledResult = new DiagnosticDescriptor(
        id: "CYRUS012",
        title: "Result tuple element should be called Result",
        messageFormat: "Invalid tuple element name {0} for Result",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Invalid tuple element name."
    );
    public static readonly DiagnosticDescriptor MessagesTupleElementShouldBeCalledMessages = new DiagnosticDescriptor(
        id: "CYRUS014",
        title: "Messages tuple element should be called Messages",
        messageFormat: "Invalid tuple element name {0} for Messages",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Invalid tuple element name."
    );

    public static readonly DiagnosticDescriptor CommandHandlerArgumentShouldBeAnAggregateRoot = new DiagnosticDescriptor(
        id: "CYRUS015",
        title: "Invalid argument for CommandHandler",
        messageFormat: "CommandHandler '{0}' should have argument which is marked as [AggregateRoot]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Additional CommandHandler parameters must be an [AggregateRoot]."
    );

    public static readonly DiagnosticDescriptor CommandForCommandHandlerShouldHaveAggregateRootIdProperty = new DiagnosticDescriptor(
        id: "CYRUS016",
        title: "Invalid argument for CommandHandler",
        messageFormat: "Command for CommandHandler '{0}' should have a property marked with [AggregateRootId]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command used as parameter in a CommandHandler must have a [AggregateRootId] property."
    );

    public static readonly DiagnosticDescriptor CommandHandlerCantHaveMultipleAggregateRootParameters = new DiagnosticDescriptor(
        id: "CYRUS017",
        title: "Invalids arguments for CommandHandler",
        messageFormat: "Command for CommandHandler '{0}' can't have multiple parameters that are defined as [AggregateRoot]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command for CommandHandler '{0}' can't have multiple parameters that are defined as [AggregateRoot]."
    );

    public static readonly DiagnosticDescriptor CommandHandlerCantHaveMultipleCancellationTokenParameters = new DiagnosticDescriptor(
        id: "CYRUS017",
        title: "Invalid arguments for CommandHandler",
        messageFormat: "Command for CommandHandler '{0}' can't have multiple parameters of type CancellationToken",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Command for CommandHandler '{0}' can't have multiple parameters  of type CancellationToken."
    );

    public static DiagnosticDescriptor UnrecognizedParameterForCommandHandler= new DiagnosticDescriptor(
        id: "CYRUS018",
        title: "Invalid parameter type for CommandHandler",
        messageFormat: "CommandHandler '{0}' has additional parameters that are not an [AggregateRoot] or a CancellationToken",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "CommandHandler '{0}' has additional parameters that are not an [AggregateRoot] or a CancellationToken."
    );

    public static readonly DiagnosticDescriptor AggregateRootShouldHaveAggregateRootIdProperty = new DiagnosticDescriptor(
        id: "CYRUS019",
        title: "AggregateRoot should have [AggregateRootId]",
        messageFormat: "AggregateRoot '{0}' should have a property marked with [AggregateRootId]",
        category: "Cyrus",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "AggregateRoots must have a [AggregateRootId] property."
    );


}