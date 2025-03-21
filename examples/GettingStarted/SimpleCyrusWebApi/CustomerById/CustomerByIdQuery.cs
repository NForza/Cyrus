using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi.CustomerById;

[Query(Route="{Id:Guid}")]
public record struct CustomerByIdQuery(CustomerId Id);