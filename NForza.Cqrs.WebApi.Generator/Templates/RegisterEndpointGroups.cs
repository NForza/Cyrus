using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace NForza.Cqrs.WebApi;

public static class RegisterEndpointGroup 
{
    public static CqrsOptions AddEndpointGroups(this CqrsOptions options)
    {
        % RegisterEndpointGroups %
        options.Services.AddTransient<IQueryFactory, HttpContextQueryFactory>();

        return options.AddCqrsEndpoints();
    }
}
