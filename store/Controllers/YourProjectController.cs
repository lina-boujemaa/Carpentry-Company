using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Models;
using store.Services;
using System.Threading.Tasks;

namespace store.Controllers
{
    [Authorize]
    public class YourProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public YourProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /YourProject
        public IActionResult Index()
        {
            return View();
        }

        // GET: /YourProject/Renovation
        public IActionResult Renovation()
        {
            return PartialView(); // Assuming Renovation.cshtml is in Views/YourProject
        }


        // GET: /YourProject/Construction
        public IActionResult Construction()
        {
            return PartialView("~/Views/YourProject/Construction.cshtml");
        }


        // POST: /YourProject/SubmitQuote
        [HttpPost]
        public async Task<IActionResult> SubmitQuote(string name, string email, string phone, string details)
        {
            if (ModelState.IsValid)
            {
                // Create a new Quote object
                var quote = new Quote
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Details = details,
                    CreatedAt = DateTime.Now
                };

                // Add the quote to the database
                _context.Quotes.Add(quote);
                await _context.SaveChangesAsync();

                return RedirectToAction("ThankYou");
            }

            return View("Index"); // Return to the Index view if model state is invalid
        }


        // GET: /YourProject/ThankYou
        public IActionResult ThankYou()
        {
            return View();
        }
        // GET: /YourProject/ContactUs
        public IActionResult ContactUs()
        {
            return View();
        }

    }
}
