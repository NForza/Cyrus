namespace SimpleCyrusWebApi;

// Handles requests for getting a customer by Id
public static class CustomerByIdQueryHandler
{
    public static string Query(CustomerByIdQuery query)
    {
        return "Customer " + query.Id;
    }
}