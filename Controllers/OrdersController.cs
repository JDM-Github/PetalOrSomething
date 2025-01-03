using Microsoft.AspNetCore.Mvc;
using PetalOrSomething.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using Newtonsoft.Json;
using System.Text;

namespace PetalOrSomething.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OrdersController(ApplicationDbContext context, EmailService emailService, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<IActionResult> OrderHistory(int page = 1,
        string orderStatus = "All", string status = "All", string search = "")
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = int.Parse(userIdString);

            int pageSize = 10;
            var query = _context.TransactionOrders
                .Where(o => o.UserId == userId)
                .Include(o => o.Products)
                .ThenInclude(p => p.Product)
                .AsQueryable();

            if (status != "All")
            {
                query = query.Where(f => f.Status == status);
            }
            if (orderStatus != "All")
            {
                query = query.Where(f => f.OrderStatus == orderStatus);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.ReferenceNumber.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalItems ? totalItems : page * pageSize;

            var orderView = new TransactionOrderView
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                StatusFilter = status,
                OrderStatusFilter = orderStatus,
                TotalCount = totalItems,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };

            return View(orderView);
        }


        public async Task<IActionResult> CustomOrder(int page = 1,
        string orderStatus = "All", string status = "All", string search = "")
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = int.Parse(userIdString);

            int pageSize = 10;
            var query = _context.TransactionCustomOrders
                .Where(o => o.UserId == userId)
                .Include(o => o.Products)
                .AsQueryable();

            if (status != "All")
            {
                query = query.Where(f => f.Status == status);
            }
            if (orderStatus != "All")
            {
                query = query.Where(f => f.OrderStatus == orderStatus);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.ReferenceNumber.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var orderView = new TransactionCustomOrderView
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                StatusFilter = status,
                OrderStatusFilter = orderStatus,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };

            return View(orderView);
        }


        // public async Task<IActionResult> Index(int page = 1, string search = "", string sort = "newest", string statusFilter = "all")
        // {
        //     int itemsPerPage = 10;
        //     var query = _context.Orders
        //         .Include(o => o.User)
        //         .Include(o => o.Product)
        //         .AsQueryable();

        //     if (!string.IsNullOrEmpty(search))
        //     {
        //         query = query.Where(o => o.User.FullName.Contains(search) || o.Product.Name.Contains(search));
        //     }

        //     if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
        //     {
        //         query = query.Where(o => o.Status == statusFilter);
        //     }

        //     switch (sort)
        //     {
        //         case "newest":
        //             query = query.OrderByDescending(o => o.OrderDate);
        //             break;
        //         case "oldest":
        //             query = query.OrderBy(o => o.OrderDate);
        //             break;
        //         case "price_low_high":
        //             query = query.OrderBy(o => o.TotalPrice);
        //             break;
        //         case "price_high_low":
        //             query = query.OrderByDescending(o => o.TotalPrice);
        //             break;
        //     }

        //     int totalItems = await query.CountAsync();
        //     int totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

        //     var orders = await query
        //         .Skip((page - 1) * itemsPerPage)
        //         .Take(itemsPerPage)
        //         .ToListAsync();

        //     var totalCount = await query.CountAsync();
        //     var startIndex = (page - 1) * itemsPerPage + 1;
        //     var endIndex = page * itemsPerPage > totalItems ? totalItems : page * itemsPerPage;

        //     var model = new OrderViewModel
        //     {
        //         Orders = orders,
        //         SearchFilter = search,
        //         CurrentPage = page,
        //         TotalPages = totalPages,
        //         TotalItems = totalItems,
        //         ItemsPerPage = itemsPerPage,
        //         SortFilter = sort,
        //         TotalCount = totalCount,
        //         StatusFilter = statusFilter,
        //         StartIndex = startIndex,
        //         EndIndex = endIndex
        //     };

        //     return View(model);
        // }


        public async Task<IActionResult> Index(int page = 1,
        string orderStatus = "All", string status = "All", string search = "")
        {

            int pageSize = 10;
            var query = _context.TransactionOrders
                .Where(o => o.Status == "Paid")
                .Include(o => o.Products)
                .ThenInclude(p => p.Product)
                .AsQueryable();

            if (status != "All")
            {
                query = query.Where(f => f.Status == status);
            }
            if (orderStatus != "All")
            {
                query = query.Where(f => f.OrderStatus == orderStatus);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.ReferenceNumber.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var orders = await query
                .Join(_context.Account,
                    order => order.UserId,
                    account => account.Id,
                    (order, account) => new { Order = order, AccountEmail = account.Email })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(o => o.Order.CreatedAt)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var orderView = new TransactionOrderAdminView
            {
                Orders = orders.Select(o => new TransactionOrderAdmin
                {
                    Order = o.Order,
                    UserEmail = o.AccountEmail
                }).ToList(),

                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                StatusFilter = status,
                OrderStatusFilter = orderStatus,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };
            return View(orderView);
        }

        public async Task<IActionResult> AdminCustomOrder(int page = 1,
        string orderStatus = "All", string status = "All", string search = "")
        {

            int pageSize = 10;
            var query = _context.TransactionCustomOrders
                .Where(o => o.Status == "Paid")
                .Include(o => o.Products)
                .AsQueryable();

            if (status != "All")
            {
                query = query.Where(f => f.Status == status);
            }
            if (orderStatus != "All")
            {
                query = query.Where(f => f.OrderStatus == orderStatus);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.ReferenceNumber.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var orders = await query
                .Join(_context.Account,
                    order => order.UserId,
                    account => account.Id,
                    (order, account) => new { Order = order, AccountEmail = account.Email })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(o => o.Order.CreatedAt)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var orderView = new TransactionCustomOrderAdminView
            {
                Orders = orders.Select(o => new TransactionCustomOrderAdmin
                {
                    Order = o.Order,
                    UserEmail = o.AccountEmail
                }).ToList(),

                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                StatusFilter = status,
                OrderStatusFilter = orderStatus,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };

            return View(orderView);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string orderStatus)
        {
            var order = await _context.TransactionOrders.FindAsync(orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            order.OrderStatus = orderStatus;
            if (orderStatus == "Completed")
            {
                order.Status = "Paid";
            }
            _context.Update(order);
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = order.UserId,
                Title = "Order Status Updated",
                Message = $"Your order with Reference Number {order.ReferenceNumber} has been updated to {orderStatus}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var userEmail = _context.Account.FirstOrDefault(a => a.Id == order.UserId)!.Email;
            var subject = "Order Status Updated";
            var body = $"Hello, your order with Reference Number {order.ReferenceNumber} has been updated to {orderStatus}.";
            await _emailService.SendEmailAsync(userEmail, subject, body);

            TempData["SuccessMessage"] = "Order status updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCustomStatus(int orderId, string orderStatus)
        {
            var order = await _context.TransactionCustomOrders.FindAsync(orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("AdminCustomOrder");
            }
            order.OrderStatus = orderStatus;
            if (orderStatus == "Completed")
            {
                order.Status = "Paid";
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = order.UserId,
                Title = "Custom Order Status Updated",
                Message = $"Your order with Reference Number {order.ReferenceNumber} has been updated to {orderStatus}.",
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var userEmail = _context.Account.FirstOrDefault(a => a.Id == order.UserId)!.Email;
            var subject = "Custom Order Status Updated";
            var body = $"Hello, your order with Reference Number {order.ReferenceNumber} has been updated to {orderStatus}.";
            await _emailService.SendEmailAsync(userEmail, subject, body);

            TempData["SuccessMessage"] = "Custom Order status updated successfully.";
            return RedirectToAction("AdminCustomOrder");
        }


        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(int orderId, int rating, string feedback)
        {
            var order = await _context.TransactionOrders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.OrderStatus != "Completed" || order.IsFeedback)
            {
                TempData["ErrorMessage"] = "Invalid request or feedback already submitted.";
                return RedirectToAction("OrderHistory");
            }

            order.FeedBack = feedback;
            order.IsFeedback = true;
            var feedbackModel = new Feedback
            {
                Rate = rating,
                FeedbackNote = feedback,
                TransactionId = order.Id,
                IsCustomTransaction = false
            };
            _context.Feedbacks.Add(feedbackModel);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thank you for your feedback!";
            return RedirectToAction("OrderHistory");
        }

        public async Task<IActionResult> SubmitCustomFeedback(int orderId, int rating, string feedback)
        {
            var order = await _context.TransactionCustomOrders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.OrderStatus != "Completed" || order.IsFeedback)
            {
                TempData["ErrorMessage"] = "Invalid request or feedback already submitted.";
                return RedirectToAction("OrderHistory");
            }

            order.FeedBack = feedback;
            order.IsFeedback = true;

            var feedbackModel = new Feedback
            {
                Rate = rating,
                FeedbackNote = feedback,
                TransactionCustomId = order.Id,
                IsCustomTransaction = true
            };
            _context.Feedbacks.Add(feedbackModel);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thank you for your feedback!";
            return RedirectToAction("OrderHistory");
        }



        public async Task<IActionResult> PaymentSuccess(string reference)
        {
            var transaction = await _context.TransactionOrders
                .Include(t => t.Products)
                .ThenInclude(p => p.Product)
                .ThenInclude(p => p.Stocks)
                .FirstOrDefaultAsync(t => t.ReferenceNumber == reference);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found.";
                return RedirectToAction("OrderHistory");
            }

            transaction.Status = "Paid";
            var notification = new Notification
            {
                UserId = transaction.UserId,
                Title = "Payment Successful",
                Message = $"Your paid your remaining balance {transaction.RemainingBalance:C}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);

            transaction.RemainingBalance = 0;
            _context.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Remaining balance paid successfully!";
            return RedirectToAction("OrderHistory", "Orders");
        }

        public IActionResult PaymentCancelled()
        {
            TempData["ErrorMessage"] = "Payment was cancelled. Please try again.";
            return RedirectToAction("OrderHistory");
        }

        [HttpPost]
        public async Task<IActionResult> PayBalance(int orderId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var user = await _context.Account.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var order = await _context.TransactionOrders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("OrderHistory");
            }
            if (order.RemainingBalance <= 0)
            {
                TempData["ErrorMessage"] = "This order has no remaining balance.";
                return RedirectToAction("OrderHistory");
            }

            var payment = new[] {
                new
                {
                    currency = "PHP",
                    images = new string[] { "https://cdn-icons-png.flaticon.com/512/2867/2867900.png" },
                    amount = (int)(order.RemainingBalance * 100),
                    name = "Remaining Balance",
                    quantity = 1,
                    description = "Remaining balance payment",
                }
            };
            var referenceNumber = Guid.NewGuid().ToString();
            var successUrl = $"{Request.Scheme}://{Request.Host}/Orders/PaymentSuccess?reference={order.ReferenceNumber}";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/Orders/PaymentCancelled";

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        billing = new
                        {
                            address = new
                            {
                                line1 = user.Location,
                                city = "Calamba City",
                                state = "Laguna",
                                postal_code = "4027",
                                country = "PH"
                            },
                            name = user.FullName,
                            email = user.Email,
                            phone = user.PhoneNumber
                        },
                        send_email_receipt = true,
                        show_description = true,
                        show_line_items = true,
                        payment_method_types = new string[] { "qrph", "billease", "card", "dob", "dob_ubp", "brankas_bdo", "gcash", "brankas_landbank", "brankas_metrobank", "grab_pay", "paymaya" },
                        line_items = payment,
                        description = "Payment for remaining balance",
                        reference_number = referenceNumber,
                        statement_descriptor = "Petal and Planes",
                        success_url = successUrl,
                        cancel_url = cancelUrl,
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(_configuration["PayMongo:SecretKey"])));
                var response = await _httpClient.PostAsync("https://api.paymongo.com/v1/checkout_sessions", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var jsonDeserialized = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var paymentLink = jsonDeserialized?.data?.attributes?.checkout_url;
                    return Redirect(paymentLink.ToString());
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create payment link.";
                    return RedirectToAction("OrderHistory");
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your payment.";
                return RedirectToAction("OrderHistory");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.TransactionOrders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("OrderHistory");
            }

            if (order.Status != "Expired" && order.Status != "Cancelled")
            {
                TempData["ErrorMessage"] = "Only expired or cancelled orders can be deleted.";
                return RedirectToAction("OrderHistory");
            }

            if (order.Products != null)
            {
                _context.CartFinishedItems.RemoveRange(order.Products);
            }

            _context.TransactionOrders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order deleted successfully.";
            return RedirectToAction("OrderHistory");
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
