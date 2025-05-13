using CyrusDemo.Tracks;
using Microsoft.EntityFrameworkCore;

namespace CyrusDemo;

public class DemoContext: DbContext
{
    public DemoContext(DbContextOptions<DemoContext> options) : base(options)
    {
    }
    public DbSet<Track> Tracks { get; set; } = null!;
}
