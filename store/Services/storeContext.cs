// storeContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using store.Areas.Identity.Data;

namespace store.Services
{
    public class storeContext : IdentityDbContext<storeUser>
    {
        public storeContext(DbContextOptions<storeContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Remove the mapping for FirstName and LastName
        }
    }
}
