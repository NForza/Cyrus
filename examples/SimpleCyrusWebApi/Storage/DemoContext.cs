using Microsoft.EntityFrameworkCore;
using SimpleCyrusWebApi.Model;

namespace SimpleCyrusWebApi.Storage;

public class DemoContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use an in-memory database named "InMemoryDb"
        optionsBuilder.UseInMemoryDatabase("InMemoryDb");
    }
}