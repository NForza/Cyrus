using NForza.Cyrus.Abstractions;

namespace SimpleCyrusWebApi;


// This generates struct that represents a strongly typed Id for a Customer entity
// also a JSONConverter and a TypeConverter are generated
[GuidId]
public partial record struct CustomerId;