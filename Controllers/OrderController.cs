// using Microsoft.AspNetCore.Mvc;
// using YourNamespace.Models;
// using Microsoft.AspNetCore.Identity;
// using System.Linq;
// using Microsoft.EntityFrameworkCore;
// using PetalOrSomething.Data;

// namespace YourNamespace.Controllers
// {
//     public class OrderController : Controller
//     {
//         private readonly ApplicationDbContext _context;
//         // private readonly UserManager<ApplicationUser> _userManager;

//         public OrderController(ApplicationDbContext context)
//         {
//             _context = context;
//             // _userManager = userManager;
//         }

//         // GET: Order
//         public IActionResult Index()
//         {
//             var orders = _context.Orders.Include(o => o.FlowerInventory).ToList();
//             return View(orders);
//         }

//         // GET: Order/Details/5
//         public IActionResult Details(int id)
//         {
//             var order = _context.Orders.Include(o => o.FlowerInventory).FirstOrDefault(o => o.Id == id);
//             if (order == null)
//             {
//                 return NotFound();
//             }
//             return View(order);
//         }

//         // GET: Order/Create
//         public IActionResult Create()
//         {
//             return View();
//         }

//         // POST: Order/Create
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public IActionResult Create(Order order)
//         {
//             if (ModelState.IsValid)
//             {
//                 // order.UserId = _userManager.GetUserId(User);
//                 order.OrderDate = DateTime.Now;

//                 _context.Add(order);
//                 _context.SaveChanges();
//                 return RedirectToAction(nameof(Index));
//             }
//             return View(order);
//         }

//         // GET: Order/Edit/5
//         public IActionResult Edit(int id)
//         {
//             var order = _context.Orders.FirstOrDefault(o => o.Id == id);
//             if (order == null)
//             {
//                 return NotFound();
//             }
//             return View(order);
//         }

//         // POST: Order/Edit/5
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public IActionResult Edit(int id, Order order)
//         {
//             if (id != order.Id)
//             {
//                 return NotFound();
//             }

//             if (ModelState.IsValid)
//             {
//                 _context.Update(order);
//                 _context.SaveChanges();
//                 return RedirectToAction(nameof(Index));
//             }
//             return View(order);
//         }

//         // GET: Order/Delete/5
//         public IActionResult Delete(int id)
//         {
//             var order = _context.Orders.FirstOrDefault(o => o.Id == id);
//             if (order == null)
//             {
//                 return NotFound();
//             }
//             return View(order);
//         }

//         // POST: Order/Delete/5
//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public IActionResult DeleteConfirmed(int id)
//         {
//             var order = _context.Orders.FirstOrDefault(o => o.Id == id);
//             if (order == null)
//             {
//                 return NotFound();
//             }

//             _context.Orders.Remove(order);
//             _context.SaveChanges();
//             return RedirectToAction(nameof(Index));
//         }
//     }
// }
