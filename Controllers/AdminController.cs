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

        public IActionResult View3D(string modelUrl, string note, string customization)
        {
            ViewData["ModelUrl"] = modelUrl;
            ViewData["Note"] = note;
            ViewData["Customization"] = customization;
            return View();
        }

        public IActionResult Index()
        {
            var flowerInventories = _context.FlowerInventories
                .Include(f => f.Stocks)
                .ToList();

            var transactionOrders = _context.TransactionCustomOrders
                .Where(t => t.Status == "Paid")
                .Include(t => t.FinishedProducts)
                .ThenInclude(p => p.Product)
                .ToList();

            var transactionCustomOrders = _context.TransactionCustomOrders
                .Where(t => t.Status == "Paid")
                .Include(t => t.Products)
                .ToList();

            var totalFlowers = flowerInventories.Count;
            var totalStock = flowerInventories.Sum(f => f.Stocks.Sum(s => s.Quantity));
            var totalExpiredStock = flowerInventories
                .SelectMany(f => f.Stocks)
                .Where(s => s.ExpiryDate <= DateTime.Now)
                .Sum(s => s.Quantity);

            var expiredPercentage = totalStock > 0 ? totalExpiredStock * 100 / totalStock : 0;
            var notExpiredPercentage = 100 - expiredPercentage;

            var mostPopularProducts = transactionOrders
                .SelectMany(t => t.FinishedProducts)
                .GroupBy(p => p.Product)
                .Select(g => new
                {
                    Product = g.Key,
                    TotalQuantity = g.Sum(p => p.Quantity),
                    TotalSales = g.Sum(p => p.TotalPrice)
                })
                .OrderByDescending(g => g.TotalQuantity)
                .Take(5)
                .ToList();
            
            Console.WriteLine("--------------------------");
            Console.WriteLine(mostPopularProducts);
            mostPopularProducts.ForEach(p => Console.WriteLine(p.Product.Name));
            Console.WriteLine("--------------------------");


            var recentTransactions = transactionOrders
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalFlowers = totalFlowers,
                TotalStock = totalStock,
                TotalExpiredStock = totalExpiredStock,
                ExpiredPercentage = expiredPercentage,
                NotExpiredPercentage = notExpiredPercentage,
                TotalRevenue = transactionOrders.Sum(t => t.TotalAmount) + transactionCustomOrders.Sum(t => t.TotalAmount),
                MostPopularProducts = mostPopularProducts.Select(p => new PopularProductViewModel
                {
                    Name = p.Product.Name,
                    TotalQuantity = p.TotalQuantity,
                    TotalSales = p.TotalSales
                }).ToList(),
                RecentTransactions = recentTransactions.Select(t => new TransactionViewModel
                {
                    TransactionId = t.Id,
                    Order = t,
                    Total = t.TotalAmount,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return View(model);
        }

        public IActionResult AddProduct(string productName, string modelUrl)
        {
            return RedirectToAction("Create", "Inventory", new { productName, modelUrl });
        }

    }
}
