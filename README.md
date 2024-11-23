# CqrsGen - an opinionated framework for creating CQRS applications

CqrsGen follows a few practices from [Domain Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design) to create WebApi's quickly and pragmatically. CqrsGen is using [Roslyn Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md) to generate code rather than rely on Reflection.

CqrsGen generates code for the following parts:

* Strongly Typed IDs
* Query Handlers
* Command Handlers 

### Strongly Typed IDs

CqrsGen lets you define strongly typed IDs by defining a `record struct` like this:

```csharp
[StringId(3, 200)]
public partial record struct Address;
```

This will generate additional code that simplifies using that type in code, like a custom JsonConverter, a TypeConverter, casting operators and other methods.

```csharp
[JsonConverter(typeof(AddressJsonConverter))]
[TypeConverter(typeof(AddressTypeConverter))]
public partial record struct Address(string Value): ITypedId
{
    public static Address Empty => new Address(string.Empty);
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
    public static implicit operator string(DemoApp.Contracts.Address typedId) => typedId.Value;
    public static explicit operator DemoApp.Contracts.Address(string value) => new(value);
    public bool IsValid() => !string.IsNullOrEmpty(Value) && Value.Length >= 3 && Value.Length <= 200;
    public override string ToString() => Value?.ToString();
}
```

The other supported type is a strongly typed ID based on a GUID:

```csharp
[GuidId]
public partial record struct CustomerId;
```