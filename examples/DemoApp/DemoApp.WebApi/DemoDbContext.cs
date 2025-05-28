using DemoApp.Domain.Customer;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.WebApi;

public class DemoDbContext: DbContext
{

    public DemoDbContext(DbContextOptions<DemoDbContext> options): base(options)
    {
    }
    public DbSet<Customer> Customers { get; set; }
}
