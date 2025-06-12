using Microsoft.EntityFrameworkCore;
using NForza.Cyrus.Abstractions;
using SimpleCyrusWebApi.Model;
using SimpleCyrusWebApi.Storage;

namespace SimpleCyrusWebApi.CustomerById;

public class CustomerByIdQueryHandler(DemoContext context)
{
    [QueryHandler(Route = "{Id}")]
    public async Task<Customer?> Query(CustomerByIdQuery query)
        => await context.Customers.FirstOrDefaultAsync(c => c.Id == query.Id);
}