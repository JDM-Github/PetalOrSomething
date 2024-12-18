using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;

namespace PetalOrSomething.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateOrder(List<int> cartItemIds)
        {
            if (cartItemIds == null || !cartItemIds.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                CartItemIds = cartItemIds
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }

}
