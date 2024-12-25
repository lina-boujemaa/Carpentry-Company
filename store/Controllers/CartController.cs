using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using store.Models;
using store.Services;
using System.Linq;
using System.Threading.Tasks;
using store.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace store.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<storeUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<storeUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Action to add a product to the cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == user.Id);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = user.Id
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

        // Action to view the cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cartItems = await _context.CartItems
                                    .Where(c => c.UserId == user.Id)
                                    .Include(c => c.Product) // Ensure that the Product is included
                                    .ToListAsync();

            return View(cartItems); // Passing CartItem objects directly
        }

        // Action to remove an item from the cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == user.Id);

            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
