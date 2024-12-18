using InquiryManagementApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using PetalOrSomething.ViewModels;
using System.Diagnostics;

namespace PetalOrSomething.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        public readonly FileUploadService _fileUploadService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            Console.WriteLine($"UserId: {ViewData["UserId"]}");
            _logger.LogInformation($"UserId: {ViewData["UserId"]}");

            if (userId == "admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        public IActionResult ProductEdit()
        {
            var flowerInventories = _context.FlowerInventories
            .Select(f => new
            {
                f.Price,
                f.Name,
                f.Model3DLink
            })
            .ToList();

            var flowerModels = flowerInventories.Select(f => new
            {
                price = f.Price,
                name = f.Name,
                modelPath = f.Model3DLink
            }).ToList();

            ViewBag.FlowerModels = flowerModels;
            return View();
        }

        public IActionResult OrderHere()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult View3D()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult Privacy()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult About()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult ProductView(int id)
        {
            var product = _context.FlowerInventories
                                .Include(p => p.Stocks)
                                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GetProducts()
        {
            var products = _context.FlowerInventories
                .Select(f => new
                {
                    f.TotalStock,
                    f.Id,
                    f.Name,
                    f.Model3DLink,
                    f.Price,
                    Rating = 4.5
                }).ToList();

            return Json(products);
        }


        [HttpPost]
        public IActionResult AddItem(int productId, int quantity)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("CartFinished", "Cart");
            }
            var cartFinished = new CartFinished(
                int.Parse(userId), productId, quantity
            );
            _context.CartFinishedItems.Add(cartFinished);
            _context.SaveChanges();
            return RedirectToAction("CartFinished", "Cart");
        }

        [HttpPost]
        public IActionResult PlaceOrder(int productId, int quantity)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("CartFinished", "Cart");
            }

            var product = _context.FlowerInventories.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return BadRequest("Insufficient stock or invalid product.");
            }

            var totalPrice = product.Price * quantity;
            var order = new Order
            {
                UserId = int.Parse(userId),
                ProductId = productId,
                Quantity = quantity,
                TotalPrice = totalPrice
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }



        // [HttpPost]
        // public IActionResult AddItem(int ProductId, int Quantity)
        // {
        //     Console.WriteLine("--------------------------------------------------------------");
        //     Console.WriteLine(ProductId);
        //     Console.WriteLine("--------------------------------------------------------------");

        //     return Ok(new { message = "Product added to cart successfully!" });

        // var userId = HttpContext.Session.GetString("UserId");
        // if (string.IsNullOrEmpty(userId))
        // {
        //     return Unauthorized();
        // }

        // Console.WriteLine(item);
        // return Ok(new { message = "Product added to cart successfully!" });




        // var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
        // if (cart == null)
        // {
        //     cart = new Cart { UserId = userId };
        //     _context.Carts.Add(cart);
        //     _context.SaveChanges();
        // }

        // var existingItem = _context.CartFinishedItems.FirstOrDefault(i => i.CartId == cart.Id && i.ProductId == item.ProductId);

        // if (existingItem != null)
        // {
        //     existingItem.Quantity += item.Quantity;
        // }
        // else
        // {
        //     item.CartId = cart.Id;
        //     _context.CartFinishedItems.Add(item);
        // }

        //     _context.SaveChanges();
        //     return Ok(new { message = "Product added to cart successfully!" });
        // }

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

        [HttpPost]
        public async Task<IActionResult> AddToCart(IFormFile GLBFile, string Name, int Quantity)
        {
            var userid = HttpContext.Session.GetString("UserId");
            var model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(GLBFile);
            var newCart = new CartItem(model3DLink, int.Parse(userid!), Name, Quantity);
            _context.CartItems.Add(newCart);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");

            // var userId = HttpContext.Session.GetString("UserId");
            // if (string.IsNullOrEmpty(userId))
            // {
            //     return RedirectToAction("Index", "Home");
            // }
            // var cart = _context.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
            // string model3DLink = "";
            // Console.WriteLine("TEST1");
            // if (GLBFile != null && GLBFile.Length > 0)
            // {
            //     Console.WriteLine(model3DLink);
            //     model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(GLBFile);
            // }
            // Console.WriteLine("TEST2");
            // Console.WriteLine(model3DLink);
            // double price = CalculatePrice(orderName);
            // var cartItem = new CartItem
            // {
            //     ProductName = orderName,
            //     Model3DLink = model3DLink,
            //     Price = price,
            //     Quantity = quantity,
            //     CartId = cart!.Id
            // };

            // cart.Items.Add(cartItem);
            // _context.Update(cart);
            // await _context.SaveChangesAsync();
            // Console.Write("GOOD");
            // return RedirectToAction("Index", "Home");
        }

        private double CalculatePrice(string orderName)
        {
            throw new NotImplementedException();
        }
    }
}
