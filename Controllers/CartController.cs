using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using PetalOrSomething.ViewModels;


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

        public IActionResult CartItem()
        {
            var cartItems = _context.CartItems
            .Join(
                _context.Account,
                cart => cart.UserId,
                user => user.Id,
                (cart, user) => new CartItemWithUserViewModel
                {
                    Model3DLink = cart.Model3DLink,
                    Id = cart.Id,
                    UserId = cart.UserId,
                    UserFirstName = user.FirstName,
                    UserLastName = user.LastName,
                    UserEmail = user.Email,
                    Quantity = cart.Quantity,
                    ProductName = cart.ProductName,
                    ProductPrice = (decimal)cart.Price,
                })
            .ToList();
            return View(cartItems);
        }

        public IActionResult CartFinished()
        {
            var cartItems = _context.CartFinishedItems
                .Join(
                    _context.FlowerInventories,
                    cart => cart.ProductId,
                    product => product.Id,
                    (cart, product) => new CartItemViewModel
                    {
                        Id = cart.Id,
                        UserId = cart.UserId,
                        ProductId = cart.ProductId,
                        Quantity = cart.Quantity,
                        ProductName = product.Name,
                        ProductPrice = product.Price,
                        ProductDescription = product.Description
                    }
                )
                .ToList();

            return View(cartItems);
        }


        // public IActionResult Index()
        // {
        //     var userId = HttpContext.Session.GetString("UserId");
        //     if (string.IsNullOrEmpty(userId))
        //     {
        //         return RedirectToAction("Login", "Accounts");
        //     }

        //     var cart = _context.Carts
        //         .Where(c => c.UserId == userId)
        //         .Select(c => new Cart
        //         {
        //             Id = c.Id,
        //             UserId = c.UserId,
        //             Items = _context.CartItems.Where(i => i.CartId == c.Id).ToList()
        //         })
        //         .FirstOrDefault();

        //     return View(cart ?? new Cart { UserId = userId });
        // }




    }
}
