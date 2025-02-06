using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi.CustomerById;

[Query]
public record struct CustomerByIdQuery(CustomerId Id);