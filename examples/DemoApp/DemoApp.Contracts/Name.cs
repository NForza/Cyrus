using NForza.Cyrus.Abstractions;

namespace DemoApp.Contracts;

[StringValue(1, 50, "^[A-Za-z ]*$")]
public partial record struct Name;
