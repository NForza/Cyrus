namespace SimpleCyrusWebApi;

// Handles requests for getting a customer by Id
public class CustomerByIdQueryHandler
{
    public static string Query(CustomerByIdQuery query)
    {
        return "Customer " + query.Id;
    }
}