// storeUser.cs
using Microsoft.AspNetCore.Identity;
using store.Models;

namespace store.Areas.Identity.Data
{
    public class storeUser : IdentityUser
    {
        // Remove FirstName and LastName properties
        // Example: navigation property for CartItems (optional)
        public ICollection<CartItem> CartItems { get; set; }
    }
}
