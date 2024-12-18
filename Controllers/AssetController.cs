using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using System.Linq;

namespace PetalOrSomething.Controllers
{
    public class AssetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Asset
        public IActionResult Index()
        {
            var assets = _context.Assets.ToList();
            return View(assets);
        }

        // GET: Asset/Add
        // GET: Asset/Add
        public IActionResult Add()
        {
            return View(new Asset());
        }

        // POST: Asset/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Asset asset)
        {
            if (ModelState.IsValid)
            {
                _context.Assets.Add(asset);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // GET: Asset/Edit/5
        public IActionResult Edit(int id)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        // POST: Asset/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Asset asset)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(asset);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // GET: Asset/Delete/5
        public IActionResult Delete(int id)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        // POST: Asset/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            _context.Assets.Remove(asset);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Asset/Details/5
        public IActionResult Details(int id)
        {
            var asset = _context.Assets.FirstOrDefault(a => a.Id == id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }
    }
}
