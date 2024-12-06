﻿
using NForza.Cyrus.WebApi.Policies;

namespace NForza.Cyrus.WebApi;

public record QueryEndpointDefinition(Type QueryType) : EndpointDefinition(QueryType)
{
    public List<QueryResultPolicy> QueryResultPolicies { get; internal set; } = [];
}