namespace DemoApp.Contracts.Customers;

public record struct AllCustomersQuery(int page = 1 , long pageSize = 10);
