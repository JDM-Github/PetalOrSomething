using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Models;
using PetalOrSomething.Data;
using Microsoft.EntityFrameworkCore;
using InquiryManagementApp.Service;

namespace PetalOrSomething.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public readonly FileUploadService _fileUploadService;
        public InventoryController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: Inventory
        public async Task<IActionResult> Index(int page = 1, string category = "All", string search = "", string isActive = "")
        {
            int pageSize = 10;
            var query = _context.FlowerInventories.AsQueryable();
            if (category != "All")
            {
                query = query.Where(f => f.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search) || f.Category.Contains(search));
            }

            switch (isActive)
            {
                case "active":
                    query = query.Where(a => a.IsAvailable);
                    break;
                case "inactive":
                    query = query.Where(a => !a.IsAvailable);
                    break;
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var inventories = await query
                .Include(f => f.Stocks)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var model = new InventoryViewModel
            {
                Inventories = inventories,
                CurrentPage = page,
                TotalPages = totalPages,
                CategoryFilter = category,
                SearchFilter = search,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex,
                FilterAvailable = isActive
            };
            var almostWitheredInventories = await _context.FlowerInventories
                .Include(e => e.Stocks)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    TotalStock = e.Stocks.Sum(s => s.Quantity)
                })
                .Where(x => x.TotalStock < 100)
                .ToListAsync();

            ViewBag.AlmostWithered = almostWitheredInventories;


            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FlowerInventory model, IFormFile? Model3DFile, IFormFile? Model2DImage)
        {
            if (ModelState.IsValid)
            {
                if (await _context.FlowerInventories.AnyAsync(f => f.Name == model.Name))
                {
                    TempData["ErrorMessage"] = "Flower already exists.";
                    return View(model);
                }
                if (Model3DFile != null)
                {
                    var model3D = await _fileUploadService.UploadFileToCloudinaryAsync(Model3DFile);
                    model.Model3DLink = model3D;
                }

                if (Model2DImage != null)
                {
                    var model2D = await _fileUploadService.UploadFileToCloudinaryAsync(Model2DImage);
                    model.Model2DLink = model2D;
                }

                _context.FlowerInventories.Add(model);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _context.FlowerInventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FlowerInventory model, IFormFile? Model3DFile = null, IFormFile? Model2DImage = null)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInventory = await _context.FlowerInventories.FindAsync(id);
                    if (existingInventory == null)
                    {
                        TempData["ErrorMessage"] = "Inventory not found.";
                        return RedirectToAction("Index");
                    }
                    existingInventory.Name = model.Name;
                    existingInventory.Category = model.Category;
                    existingInventory.Price = model.Price;

                    if (Model3DFile != null)
                    {
                        var model3D = await _fileUploadService.UploadFileToCloudinaryAsync(Model3DFile);
                        existingInventory.Model3DLink = model3D;
                    }

                    if (Model2DImage != null)
                    {
                        var model2D = await _fileUploadService.UploadFileToCloudinaryAsync(Model2DImage);
                        existingInventory.Model2DLink = model2D;
                    }

                    _context.Update(existingInventory);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Inventory updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FlowerInventories.Any(e => e.Id == id))
                    {
                        TempData["ErrorMessage"] = "Inventory not found.";
                        return RedirectToAction("Index");
                    }
                    throw;
                }
            }
            TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _context.FlowerInventories.FindAsync(id);
            if (inventory == null)
            {
                TempData["ErrorMessage"] = "Inventory not found.";
                return RedirectToAction("Index");
            }
            inventory.IsAvailable = !inventory.IsAvailable;
            _context.Update(inventory);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Asset availability changed successfully! {inventory.Name} is now {(inventory.IsAvailable ? "Available" : "Unavailable")}";
            return RedirectToAction("Index");
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
