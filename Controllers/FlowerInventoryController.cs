using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetalOrSomething.Controllers
{
    public class FlowerInventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlowerInventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var inventory = _context.FlowerInventories.Include(i => i.Stocks).ToList();
            return View(inventory);
        }

        [HttpGet]
        public IActionResult GetStocks(int inventoryId)
        {
            var inventory = _context.FlowerInventories
                .Where(i => i.Id == inventoryId)
                .Select(i => i.Stocks)
                .FirstOrDefault();

            return Json(inventory);
        }

        [HttpPost]
        public IActionResult AddStock(int inventoryId, int quantity, DateTime expiryDate)
        {
            var inventory = _context.FlowerInventories.FirstOrDefault(i => i.Id == inventoryId);
            if (inventory == null)
            {
                return NotFound();
            }

            var stock = new FlowerStock
            {
                Quantity = quantity,
                ExpiryDate = expiryDate,
                FlowerInventoryId = inventoryId
            };

            inventory.Stocks.Add(stock);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RemoveStock(int stockId)
        {
            if (stockId <= 0)
            {
                return BadRequest("Invalid stock ID.");
            }
            var stock = _context.FlowerStocks.FirstOrDefault(s => s.Id == stockId);
            if (stock == null)
            {
                return NotFound("Stock not found for the given ID.");
            }
            _context.FlowerStocks.Remove(stock);
            _context.SaveChanges();
            return Json(new { success = true, message = "Stock removed successfully!" });
        }

        [HttpPost]
        public IActionResult AddItem([FromBody] Cart item)
        {
            return Json(new { success = true, message = "Stock removed successfully!" });
        }


    }
}