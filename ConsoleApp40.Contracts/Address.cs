using NForza.TypedIds;

namespace DemoApp.Contracts;

[TypedId]
public partial record struct Address(string Value);
