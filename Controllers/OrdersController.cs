using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;

namespace PetalOrSomething.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                var product = _context.FlowerInventories.FirstOrDefault(p => p.Id == order.ProductId);
                if (product != null)
                {
                    product.Stocks.Add(new FlowerStock
                    {
                        Quantity = order.Quantity,
                        ExpiryDate = DateTime.Now.AddMonths(1),
                        FlowerInventoryId = product.Id
                    });
                }

                _context.Orders.Remove(order);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }

}
