using store.Areas.Identity.Data;

namespace store.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }

        // Foreign key to associate with the user
        public string UserId { get; set; }
        public storeUser User { get; set; } // Navigation property for the storeUser
    }
}
