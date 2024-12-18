using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using System.Diagnostics;

namespace PetalOrSomething.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var flowerInventories = _context.FlowerInventories
                .Include(f => f.Stocks)
                .ToList();

            var totalFlowers = flowerInventories.Count;
            var totalStock = flowerInventories.Sum(f => f.Stocks.Sum(s => s.Quantity));

            var totalExpiredStock = flowerInventories
                .SelectMany(f => f.Stocks)
                .Where(s => s.ExpiryDate <= DateTime.Now)
                .Sum(s => s.Quantity);

            var expiredPercentage = totalStock > 0 ? (totalExpiredStock * 100) / totalStock : 0;
            var notExpiredPercentage = 100 - expiredPercentage;

            var model = new AdminDashboardViewModel
            {
                TotalFlowers = totalFlowers,
                TotalStock = totalStock,
                TotalExpiredStock = totalExpiredStock,
                ExpiredPercentage = expiredPercentage,
                NotExpiredPercentage = notExpiredPercentage
            };

            return View(model);
        }
    
    }
}
