using CarWashApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarWashApp;
public class CarWashAppDbContext : DbContext
{
    public CarWashAppDbContext(DbContextOptions<CarWashAppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .Property(t => t.PaymentMethod)
            .HasConversion<string>();
    }

    public DbSet<Car> Cars { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}
