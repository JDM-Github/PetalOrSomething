using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Models;

namespace PetalOrSomething.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public required DbSet<Account> Account { get; set; } = default!;
        public required DbSet<Cart> Carts { get; set; }
        public required DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }
        public DbSet<Product> Product { get; set; } = default!;
        public required DbSet<Inventory> Inventories { get; set; }

        public required DbSet<FlowerInventory> FlowerInventories { get; set; }
        public required DbSet<FlowerStock> FlowerStocks { get; set; }
        public required DbSet<Asset> Assets { get; set; }
    }
}
