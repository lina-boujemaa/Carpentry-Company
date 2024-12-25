using Microsoft.EntityFrameworkCore;
using store.Models;
using store.Areas.Identity.Data; // Import storeUser

namespace store.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the CartItem relationship with storeUser
            builder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany() // Each user can have many cart items
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete when user is deleted
        }
    }
}
