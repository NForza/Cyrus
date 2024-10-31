using DemoApp.Contracts.Customers;

namespace DemoApp.Domain.Customer;

public class CustomersQueryHandler
{
    public List<Customer> Query(AllCustomersQuery query)
    {
        return [];
    }

    public Customer Query(CustomerByIdQuery query)
    {
        return new(query.Id, "Customer-"+ query.Id);
    }
}
