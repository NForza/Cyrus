using NForza.TypedIds;

namespace DemoApp.Contracts;

[StringId(minimumLength:1, maximumLength:50)]
public partial record struct Name;
