using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Data;
using PetalOrSomething.Models;


namespace PetalOrSomething.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddItem([FromBody] CartItem item)
        {
            return Ok();
            // Console.WriteLine(id);
            // var userId = HttpContext.Session.GetString("UserId");
            // if (string.IsNullOrEmpty(userId))
            // {
            //     return Unauthorized();
            // }

            // var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            // if (cart == null)
            // {
            //     cart = new Cart { UserId = userId };
            //     _context.Carts.Add(cart);
            //     _context.SaveChanges();
            // }

            // var existingItem = _context.CartItems.FirstOrDefault(i => i.CartId == cart.Id && i.ProductId == item.ProductId);
            // if (existingItem != null)
            // {
            //     existingItem.Quantity += item.Quantity;
            // }
            // else
            // {
            //     item.CartId = cart.Id;
            //     _context.CartItems.Add(item);
            // }

            // _context.SaveChanges();
            return Ok(new { message = "Product added to cart successfully!" });
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var cart = _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => new Cart
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Items = _context.CartItems.Where(i => i.CartId == c.Id).ToList()
                })
                .FirstOrDefault();

            return View(cart ?? new Cart { UserId = userId });
        }
    }
}
