namespace SimpleCyrusWebApi;

// Handles requests for getting a customer by Id
public class CustomerByIdQueryHandler
{
    public Task<string> Query(CustomerByIdQuery query)
    {
        return Task.FromResult("Customer " + query.Id);
    }
}