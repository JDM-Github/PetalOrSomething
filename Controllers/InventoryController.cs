using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Models;
using PetalOrSomething.Data;
using Microsoft.EntityFrameworkCore;

namespace PetalOrSomething.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inventory
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.FlowerInventories
                .Include(f => f.Stocks)
                .ToListAsync();
            return View(inventories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FlowerInventory model)
        {
            if (ModelState.IsValid)
            {
                _context.FlowerInventories.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductName,Description,Price,StockQuantity,Category")] Inventory inventory)
        {
            if (id != inventory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryExists(inventory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.Id == id);
        }


        [HttpPost]
        public IActionResult AddStock(int inventoryId, int quantity, DateTime expiryDate)
        {
            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }

            if (expiryDate <= DateTime.Now)
            {
                return BadRequest("Expiry date must be in the future.");
            }

            var flowerInventory = _context.FlowerInventories
                .Include(f => f.Stocks)
                .FirstOrDefault(f => f.Id == inventoryId);

            if (flowerInventory == null)
            {
                return NotFound($"FlowerInventory with ID {inventoryId} not found.");
            }

            var newStock = new FlowerStock
            {
                Quantity = quantity,
                ExpiryDate = expiryDate,
                FlowerInventoryId = inventoryId
            };
            flowerInventory.Stocks.Add(newStock);
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok();
        }



        [HttpPost]
        public IActionResult RemoveStock(int stockId)
        {
            var stock = _context.FlowerStocks.FirstOrDefault(s => s.Id == stockId);

            if (stock == null)
            {
                return NotFound("Stock not found." + stockId);
            }

            if (stock.ExpiryDate > DateTime.Now)
            {
                return BadRequest("Cannot remove stock that is not expired.");
            }

            _context.FlowerStocks.Remove(stock);
            _context.SaveChanges();

            return Json(new { success = true, message = "Stock removed successfully!" });
        }


    }
}
