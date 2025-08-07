using Microsoft.EntityFrameworkCore;
using BurbujApp.Domain;

namespace BurbujApp.Inventory;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    // DbSets will be added here based on domain entities
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Entity configurations will be added here
    }
}