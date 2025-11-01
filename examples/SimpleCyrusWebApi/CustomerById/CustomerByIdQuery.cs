using NForza.Cyrus.Abstractions;
using SimpleCyrusWebApi.Model;

namespace SimpleCyrusWebApi.CustomerById;

[Query]
public record struct CustomerByIdQuery(CustomerId Id);