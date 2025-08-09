using CarWashApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarWashApp;
public class CarWashAppDbContext : DbContext
{
    public CarWashAppDbContext(DbContextOptions<CarWashAppDbContext> options) : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }
}
