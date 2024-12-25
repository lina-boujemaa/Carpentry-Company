using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using store.Models;
using store.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using store.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace store.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<storeUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<storeUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Allow all users (including clients) to view the list of products
        [AllowAnonymous]
        public IActionResult Index(string searchTerm)
        {
            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Brand.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.Category.Contains(searchTerm));
            }

            var products = productsQuery.ToList();
            return View(products);
        }

        // Admins can create a new product
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // Handle Create product (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null || productDto.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "The image is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            var fileName = Path.GetFileName(productDto.ImageFile.FileName);
            var filePath = Path.Combine(_environment.WebRootPath, "products", fileName);

            var directoryPath = Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await productDto.ImageFile.CopyToAsync(fileStream);
            }

            var product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = fileName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Admins can edit existing products
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ProductDto productDto)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(productDto.ImageFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "products", fileName);

                var directoryPath = Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(fileStream);
                }

                product.ImageFileName = fileName;
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Admins can delete products
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Delete the image file if it exists
            var filePath = Path.Combine(_environment.WebRootPath, "products", product.ImageFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Clients can add products to the cart
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            var existingCartItem = _context.CartItems.FirstOrDefault(c => c.ProductId == productId && c.UserId == user.Id);

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
            return RedirectToAction("Index");
        }

        // Admins can promote a user to Admin role
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Add user to the Admin role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction("AdminPanel"); // Redirect back to the admin panel
        }

        // Admins can view all users and manage admin roles
        [Authorize(Roles = "Admin")]
        public IActionResult AdminPanel()
        {
            var users = _userManager.Users.ToList();
            return View(users); // Pass the list of users to the view
        }
    }
}
