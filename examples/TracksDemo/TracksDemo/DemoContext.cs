using Microsoft.EntityFrameworkCore;
using TracksDemo.Tracks;

namespace TracksDemo;

public class DemoContext : DbContext
{
    public DemoContext(DbContextOptions<DemoContext> options) : base(options)
    {
    }
    public DbSet<Track> Tracks { get; set; } = null!;
}
