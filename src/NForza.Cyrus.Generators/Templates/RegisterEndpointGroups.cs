﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.Abstractions;

namespace NForza.Cyrus.WebApi;

public static class AddEndpointGroupExtensions
{
    public static CyrusOptions AddMessagingServices(this CyrusOptions options)
    {
        options.Services.AddScoped<ICqrsFactory, HttpContextCqrsFactory>();
        return options;
    }

    public static CyrusOptions AddEndpointGroups(this CyrusOptions options)
    {
        % RegisterEndpointGroups %
        options.AddMessagingServices().AddTypedIdSerializers();

        return options.AddCqrsEndpoints();
    }
}
