using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using NForza.Cyrus.Cqrs;

namespace NForza.Cyrus.WebApi;

public static class AddEndpointGroupExtensions
{
    public static CyrusOptions AddEndpointGroups(this CyrusOptions options)
    {
        % RegisterEndpointGroups %
        options.Services.AddScoped<ICqrsFactory, HttpContextCqrsFactory>();

        return options.AddCqrsEndpoints();
    }
}
