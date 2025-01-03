using InquiryManagementApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;

namespace PetalOrSomething.Controllers
{
    public class AssetController : Controller
    {
        private readonly ApplicationDbContext _context;
        public readonly FileUploadService _fileUploadService;
        public AssetController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: Asset
        public async Task<IActionResult> Index(int page = 1, string search = "", string sort = "newest", string priceRange = "all")
        {
            int itemsPerPage = 10;
            var query = _context.Assets.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Name.Contains(search));
            }

            if (priceRange == "50-100")
            {
                query = query.Where(a => a.Price >= 50 && a.Price <= 100);
            }
            else if (priceRange == "100-200")
            {
                query = query.Where(a => a.Price >= 100 && a.Price <= 200);
            }
            else if (priceRange == "300+")
            {
                query = query.Where(a => a.Price >= 300);
            }

            switch (sort)
            {
                case "newest":
                    query = query.OrderByDescending(a => a.Id);
                    break;
                case "oldest":
                    query = query.OrderBy(a => a.Id);
                    break;
                case "price_low_high":
                    query = query.OrderBy(a => a.Price);
                    break;
                case "price_high_low":
                    query = query.OrderByDescending(a => a.Price);
                    break;
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var assets = await query
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * itemsPerPage + 1;
            var endIndex = page * itemsPerPage > totalItems ? totalItems : page * itemsPerPage;

            var model = new AssetViewModel
            {
                Assets = assets,
                SearchFilter = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                ItemsPerPage = itemsPerPage,
                SortFilter = sort,
                TotalCount = totalCount,
                PriceRangeFilter = priceRange,
                StartIndex = startIndex,
                EndIndex = endIndex
            };

            return View(model);
        }



        public IActionResult Add()
        {
            return View(new Asset());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAsync(Asset asset, IFormFile Model3DFile)
        {
            if (ModelState.IsValid)
            {
                if (Model3DFile != null)
                {
                    var model3D = await _fileUploadService.UploadFileToCloudinaryAsync(Model3DFile);
                    asset.Model3DLink = model3D;
                }
                _context.Assets.Add(asset);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        public IActionResult Edit(int id)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Asset model, IFormFile Model3DFile)
        {
            if (ModelState.IsValid)
            {
                var asset = await _context.Assets.FindAsync(model.Id);
                if (asset == null)
                {
                    return NotFound();
                }
                asset.Name = model.Name;
                asset.Category = model.Category;
                asset.Price = model.Price;
                if (Model3DFile != null)
                {
                    var model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(Model3DFile);
                    asset.Model3DLink = model3DLink;
                }
                _context.Assets.Update(asset);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _context.Assets.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            inventory.IsAvailable = !inventory.IsAvailable;
            _context.Update(inventory);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // public IActionResult Details(int id)
        // {
        //     var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
        //     if (asset == null)
        //     {
        //         return NotFound();
        //     }
        //     return View(asset);
        // }
    }
}
