using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using System.Diagnostics;

namespace PetalOrSomething.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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

    }
}
