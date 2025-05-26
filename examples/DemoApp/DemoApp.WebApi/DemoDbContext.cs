using DemoApp.Domain.Customer;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.WebApi;

public class DemoDbContext: DbContext
{
    public DbSet<Customer> Customers { get; set; }
}
