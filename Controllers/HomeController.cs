using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Models;
using System.Diagnostics;

namespace PetalOrSomething.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            Console.WriteLine($"UserId: {ViewData["UserId"]}");
            _logger.LogInformation($"UserId: {ViewData["UserId"]}");
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

        public IActionResult ProductView()
        {
            return View();
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
    }
}
