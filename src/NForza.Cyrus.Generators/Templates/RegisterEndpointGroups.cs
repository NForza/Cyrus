using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public static class AddEndpointGroupExtensions
{
    public static CqrsOptions AddEndpointGroups(this CqrsOptions options)
    {
        % RegisterEndpointGroups %
        options.Services.AddScoped<IQueryFactory, HttpContextQueryFactory>();

        return options.AddCqrsEndpoints();
    }
}
