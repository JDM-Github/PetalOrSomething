using System.Text;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PetalOrSomething.Data;
using PetalOrSomething.Models;

namespace PetalOrSomething.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CartController(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // [HttpPost]
        // public IActionResult AddItem([FromBody] CartItem item)
        // {
        //     return Ok();
        //     // Console.WriteLine(id);
        //     // var userId = HttpContext.Session.GetString("UserId");
        //     // if (string.IsNullOrEmpty(userId))
        //     // {
        //     //     return Unauthorized();
        //     // }

        //     // var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
        //     // if (cart == null)
        //     // {
        //     //     cart = new Cart { UserId = userId };
        //     //     _context.Carts.Add(cart);
        //     //     _context.SaveChanges();
        //     // }

        //     // var existingItem = _context.CartItems.FirstOrDefault(i => i.CartId == cart.Id && i.ProductId == item.ProductId);
        //     // if (existingItem != null)
        //     // {
        //     //     existingItem.Quantity += item.Quantity;
        //     // }
        //     // else
        //     // {
        //     //     item.CartId = cart.Id;
        //     //     _context.CartItems.Add(item);
        //     // }

        //     // _context.SaveChanges();
        //     return Ok(new { message = "Product added to cart successfully!" });
        // }

        public async Task<IActionResult> PaymentCustomSuccess(string reference)
        {
            var transaction = await _context.TransactionCustomOrders
                .Include(t => t.Products)
                .FirstOrDefaultAsync(t => t.ReferenceNumber == reference);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found.";
                return RedirectToAction("CartFinished");
            }

            try
            {
                var client = new PayPalHttpClient(PayPalConfig.GetEnvironment());

                var captureRequest = new OrdersCaptureRequest(transaction.OrderId);
                captureRequest.RequestBody(new OrderActionRequest());

                var response = await client.Execute(captureRequest);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                if (result.Status == "COMPLETED")
                {
                    transaction.Status = transaction.RemainingBalance > 0 ? "Downpayment" : "Paid";
                    transaction.OrderStatus = "Pending";
                    transaction.PaymentMethod = "Paypal";

                    var cartItemToUpdate = transaction.Products;
                    cartItemToUpdate.ForEach(c => c.IsOrdered = true);
                    await _context.SaveChangesAsync();

                    var notification = new Notification
                    {
                        UserId = transaction.UserId,
                        Title = transaction.RemainingBalance > 0 ? "Downpayment Successful" : "Payment Successful",
                        Message = transaction.RemainingBalance > 0
                            ? $"Your down payment of {transaction.DownPayment:C} was successful. Your remaining balance is {transaction.RemainingBalance:C}."
                            : $"Your payment of {transaction.TotalAmount:C} was successful. Your order is now being processed.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = transaction.RemainingBalance > 0
                        ? "Downpayment successful! Your remaining balance is noted."
                        : "Payment successful! Thank you for your order.";
                    return RedirectToAction("CustomOrder", "Orders");
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment was not completed.";
                    return RedirectToAction("CartFinished");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error capturing payment: {ex.Message}";
                return RedirectToAction("CartFinished");
            }
        }

        public async Task<IActionResult> PaymentSuccess(string reference)
        {
            var transaction = await _context.TransactionCustomOrders
                .Include(t => t.Products)
                .Include(t => t.FinishedProducts)
                .ThenInclude(p => p.Product)
                .ThenInclude(p => p.Stocks)
                .FirstOrDefaultAsync(t => t.ReferenceNumber == reference);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found.";
                return RedirectToAction("CartFinished");
            }

            try
            {
                var client = new PayPalHttpClient(PayPalConfig.GetEnvironment());

                var captureRequest = new OrdersCaptureRequest(transaction.OrderId);
                captureRequest.RequestBody(new OrderActionRequest());

                var response = await client.Execute(captureRequest);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                if (result.Status == "COMPLETED")
                {
                    transaction.Status = transaction.RemainingBalance > 0 ? "Downpayment" : "Paid";
                    transaction.OrderStatus = "Pending";
                    transaction.PaymentMethod = "Paypal";

                    var cartItemToUpdate = transaction.FinishedProducts;
                    var cartFinishedItemToUpdate = transaction.Products;
                    foreach (var cartItem in cartItemToUpdate)
                    {
                        if (cartItem.Product != null)
                        {
                            cartItem.Product.RemoveStock(cartItem.Quantity);
                        }
                    }
                    cartItemToUpdate.ForEach(c => c.IsOrdered = true);
                    cartFinishedItemToUpdate.ForEach(c => c.IsOrdered = true);
                    await _context.SaveChangesAsync();

                    var notification = new Notification
                    {
                        UserId = transaction.UserId,
                        Title = transaction.RemainingBalance > 0 ? "Downpayment Successful" : "Payment Successful",
                        Message = transaction.RemainingBalance > 0
                            ? $"Your down payment of {transaction.DownPayment:C} was successful. Your remaining balance is {transaction.RemainingBalance:C}."
                            : $"Your payment of {transaction.TotalAmount:C} was successful. Your order is now being processed.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = transaction.RemainingBalance > 0
                        ? "Downpayment successful! Your remaining balance is noted."
                        : "Payment successful! Thank you for your order.";
                    return RedirectToAction("OrderHistory", "Orders");
                }
                else
                {
                    _context.TransactionCustomOrders.Remove(transaction);
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = "Payment was not completed.";
                    return RedirectToAction("CartFinished");
                }
            }
            catch (Exception ex)
            {
                _context.TransactionCustomOrders.Remove(transaction);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = $"Error capturing payment: {ex.Message}";
                return RedirectToAction("CartFinished");
            }
        }


        public async Task<IActionResult> PaymentCancelled(string reference)
        {
            var transaction = await _context.TransactionOrders
                .FirstOrDefaultAsync(t => t.ReferenceNumber == reference);

            if (transaction == null || transaction.Status == "Expired")
            {
                TempData["ErrorMessage"] = "This transaction has expired.";
                return RedirectToAction("CartFinished");
            }
            // transaction.Status = "Cancelled";
            // await _context.SaveChangesAsync();

            _context.TransactionOrders.Remove(transaction);
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "Payment was cancelled. Please try again.";
            return RedirectToAction("CartFinished");
        }
        public async Task<IActionResult> PaymentCustomCancelled(string reference)
        {
            var transaction = await _context.TransactionCustomOrders
                .FirstOrDefaultAsync(t => t.ReferenceNumber == reference);

            if (transaction == null || transaction.Status == "Expired")
            {
                TempData["ErrorMessage"] = "This transaction has expired.";
                return RedirectToAction("CartFinished");
            }
            transaction.Status = "Cancelled";
            await _context.SaveChangesAsync();
            TempData["ErrorMessage"] = "Payment was cancelled. Please try again.";
            return RedirectToAction("CartFinished");
        }

        public async Task<IActionResult> DeleteFinishedOrders()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartFinished");
            }

            var user = await _context.Account.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartFinished");
            }

            var cartItems = await _context.CartFinishedItems
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Selected == true)
                .Where(c => c.IsOrdered == false)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "No items in selected.";
                return RedirectToAction("CartFinished");
            }

            foreach (var cart in cartItems)
            {
                _context.CartFinishedItems.Remove(cart);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Items successfully deleted.";
            return RedirectToAction("CartFinished");
        }

        public async Task<IActionResult> DeleteOrders()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartFinished");
            }

            var user = await _context.Account.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartFinished");
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Selected == true)
                .Where(c => c.IsOrdered == false)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "No items in selected.";
                return RedirectToAction("CartFinished");
            }

            foreach (var cart in cartItems)
            {
                _context.CartItems.Remove(cart);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Items successfully deleted.";
            return RedirectToAction("CartFinished");
        }

        public async Task<IActionResult> CreateOrder(
            string willPickup = "Delivery", string paymentType = "Full", int downPayment = 25)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartFinished");
            }

            var user = await _context.Account.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartFinished");
            }

            var cartItems = await _context.CartFinishedItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Selected == true)
                .Where(c => c.IsOrdered == false)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "No items in selected.";
                return RedirectToAction("CartFinished");
            }

            var referenceNumber = $"REF-{new Random().Next(1000000000, 2000000000)}";

            double shippingFee = willPickup == "Pickup" ? 0 : 25;
            double totalAmount = cartItems.Sum(c => c.TotalPrice) + shippingFee;

            double calculatedDownPayment = paymentType == "Partial" ? totalAmount * (downPayment / 100.0) : totalAmount;
            double remainingBalance = paymentType == "Partial" ? totalAmount - calculatedDownPayment : 0;
            double downPaymentPercentage = paymentType == "Partial" ? downPayment / 100.0 : 1.0;

            double firstPayment = 0;
            foreach (var item in cartItems)
            {
                firstPayment += item.TotalPrice * downPaymentPercentage;
            }
            if (shippingFee > 0)
            {
                firstPayment += 25;
            }

            var orderRequest = new OrdersCreateRequest();
            orderRequest.Prefer("return=representation");
            orderRequest.RequestBody(new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = Url.Action("PaymentSuccess", "Cart", new { reference = referenceNumber }, Request.Scheme),
                    CancelUrl = Url.Action("PaymentCancelled", "Cart", new { reference = referenceNumber }, Request.Scheme),
                    UserAction = "PAY_NOW"
                },
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "PHP",
                            Value = firstPayment.ToString("F2")
                        },
                        Description = "Balance Fee"
                    }
                }
            });

            try
            {
                var client = new PayPalHttpClient(PayPalConfig.GetEnvironment());
                var response = await client.Execute(orderRequest);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                var approvalUrl = result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
                if (approvalUrl != null)
                {
                    var transaction = new TransactionOrder
                    {
                        UserId = user.Id,
                        OrderId = result.Id,
                        Products = cartItems,
                        ReferenceNumber = referenceNumber,
                        TransactionId = Guid.NewGuid().ToString(),
                        TotalAmount = totalAmount,
                        ShippingFee = shippingFee,
                        WillPickUp = willPickup == "Pickup",
                        PaymentLink = approvalUrl,
                        DownPayment = calculatedDownPayment,
                        RemainingBalance = remainingBalance,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        ExpirationTime = DateTime.UtcNow.AddMinutes(15)
                    };
                    _context.TransactionOrders.Add(transaction);
                    _context.SaveChanges();
                    return Redirect(approvalUrl);
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment approval URL not found.";
                    return RedirectToAction("PaymentHistory");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during payment: {ex.Message}";
                return RedirectToAction("PaymentHistory");
            }
        }


        public async Task<IActionResult> CreateCustomOrder(
            string willPickup = "Delivery", string paymentType = "Full", int downPayment = 20)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartItem");
            }

            var user = await _context.Account.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("CartItem");
            }

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Selected == true)
                .Where(c => c.IsOrdered == false)
                .ToListAsync();

            var cartItemsFinished = await _context.CartFinishedItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .Where(c => c.Selected == true)
                .Where(c => c.IsOrdered == false)
                .ToListAsync();

            if (cartItems.Count == 0 && cartItemsFinished.Count == 0)
            {
                TempData["ErrorMessage"] = "No items in selected.";
                return RedirectToAction("CartItem");
            }

            var referenceNumber = $"REF-{new Random().Next(1000000000, 2000000000)}";


            double shippingFee = willPickup == "Pickup" ? 0 : 25;
            double totalAmount = cartItems.Sum(c => c.TotalPrice * c.Quantity) + cartItemsFinished.Sum(c => c.TotalPrice) + shippingFee;

            double calculatedDownPayment = paymentType == "Partial" ? totalAmount * (downPayment / 100.0) : totalAmount;
            double remainingBalance = paymentType == "Partial" ? totalAmount - calculatedDownPayment : 0;
            double downPaymentPercentage = paymentType == "Partial" ? downPayment / 100.0 : 1.0;

            double firstPayment = 0;
            foreach (var item in cartItems)
            {
                firstPayment += item.TotalPrice * item.Quantity * downPaymentPercentage;
            }
            foreach (var item in cartItemsFinished)
            {
                firstPayment += item.TotalPrice * downPaymentPercentage;
            }
            if (shippingFee > 0)
            {
                firstPayment += 25;
            }

            var orderRequest = new OrdersCreateRequest();
            orderRequest.Prefer("return=representation");
            orderRequest.RequestBody(new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = Url.Action("PaymentSuccess", "Cart", new { reference = referenceNumber }, Request.Scheme),
                    CancelUrl = Url.Action("PaymentCancelled", "Cart", new { reference = referenceNumber }, Request.Scheme)
                },
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "PHP",
                            Value = firstPayment.ToString("F2")
                        },
                        Description = "Balance Fee"
                    }
                }
            });

            try
            {
                var client = new PayPalHttpClient(PayPalConfig.GetEnvironment());
                var response = await client.Execute(orderRequest);
                var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                var approvalUrl = result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
                if (approvalUrl != null)
                {
                    var transaction = new TransactionCustomOrder
                    {
                        UserId = user.Id,
                        OrderId = result.Id,
                        Products = cartItems,
                        FinishedProducts = cartItemsFinished,
                        ReferenceNumber = referenceNumber,
                        TransactionId = Guid.NewGuid().ToString(),
                        TotalAmount = totalAmount,
                        ShippingFee = shippingFee,
                        WillPickUp = willPickup == "Pickup",
                        PaymentLink = approvalUrl,
                        DownPayment = calculatedDownPayment,
                        RemainingBalance = remainingBalance,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        ExpirationTime = DateTime.UtcNow.AddMinutes(15)
                    };
                    _context.TransactionCustomOrders.Add(transaction);
                    _context.SaveChanges();
                    return Redirect(approvalUrl);
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment approval URL not found.";
                    return RedirectToAction("PaymentHistory");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during payment: {ex.Message}";
                return RedirectToAction("PaymentHistory");
            }
        }

        public async Task<IActionResult> UpdateOrder(int id, int quantity, string note)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Quantity must be greater than zero.";
                return RedirectToAction("CartFinished");
            }

            var order = await _context.CartFinishedItems.FindAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("CartFinished");
            }
            order.Quantity = quantity;
            order.Note = note ?? "";

            try
            {
                _context.CartFinishedItems.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order successfully updated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "There was an error updating the order.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("CartFinished");
            }
            return RedirectToAction("CartFinished");
        }


        // CreateCustomerOrder

        public async Task<IActionResult> CartItem(int page = 1, string search = "")
        {
            int pageSize = 10;
            var query = _context.CartItems.Where(c => c.IsOrdered == false).AsQueryable();

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }
            var user = await _context.Account.FirstOrDefaultAsync(u => u.Id == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ProductName.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var cartItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalItems ? totalItems : page * pageSize;

            var model = new CartItemViewUserModel
            {
                UserId = user!.Id,
                UserName = user.FullName,
                Email = user.Email,
                UserLocation = user.Location,
                UserPhoneNumber = user.PhoneNumber,
                Items = cartItems,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                TotalCount = totalItems,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult SetCartFinishSelected(int id)
        {
            var cartItem = _context.CartFinishedItems.FirstOrDefault(c => c.Id == id);

            if (cartItem == null)
                return BadRequest(new { message = "Cart item not found." });

            cartItem.Selected = !cartItem.Selected;
            _context.SaveChanges();

            return Ok(new { message = "Item updated successfully." });
        }

        [HttpPost]
        public IActionResult SetCartItemSelected(int id)
        {
            var cartItem = _context.CartItems.FirstOrDefault(c => c.Id == id);

            if (cartItem == null)
                return BadRequest(new { message = "Cart item not found." });

            cartItem.Selected = !cartItem.Selected;
            _context.SaveChanges();
            return Ok(new { message = "Item updated successfully." });
        }



        public async Task<IActionResult> CartFinished(int page = 1, string search = "")
        {
            int pageSize = 10;
            var query = _context.CartFinishedItems.Include(c => c.Product)
                .Where(c => c.IsOrdered == false).AsQueryable();

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

            query = query.Where(item => item.UserId == user.Id);
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Product.Name.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var cartItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var model = new CartFinishedViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.FullName,
                UserLocation = user.Location,
                UserPhoneNumber = user.PhoneNumber,
                Items = cartItems,

                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };

            return View(model);
        }

        public async Task<IActionResult> Cart(int page = 1, string search = "")
        {
            int pageSize = 10;

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

            var cartItems = await _context.CartItems
                .Where(c => c.IsOrdered == false && c.UserId == userId &&
                            (string.IsNullOrEmpty(search) || c.ProductName.Contains(search)))
                .Select(c => new UnifiedCartItem
                {
                    Id = c.Id,
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    TotalPrice = c.TotalPrice,
                    IsFinished = false,
                    Selected = c.Selected,
                    Model3DLink = c.Model3DLink,
                    Model2DLink = "",
                    Note = c.Note,
                    CustomizationJson = c.CustomizationJson
                })
                .ToListAsync();

            var finishedItems = await _context.CartFinishedItems
                .Include(c => c.Product)
                .Where(f => f.IsOrdered == false && f.UserId == userId &&
                            (string.IsNullOrEmpty(search) || f.Product.Name.Contains(search)))
                .Select(f => new UnifiedCartItem
                {
                    Id = f.Id,
                    ProductName = f.Product.Name,
                    Quantity = f.Quantity,
                    TotalPrice = f.TotalPrice,
                    IsFinished = true,
                    Selected = f.Selected,
                    Model3DLink = f.Product.Model3DLink,
                    Model2DLink = f.Product.Model2DLink,
                    Note = f.Note,
                    CustomizationJson = ""
                })
                .ToListAsync();

            var combinedItems = cartItems.Concat(finishedItems).ToList();

            int totalItems = combinedItems.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedItems = combinedItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalItems ? totalItems : page * pageSize;

            var model = new UnifiedCartViewModel
            {
                UserId = user.Id,
                UserName = user.FullName,
                Email = user.Email,
                UserLocation = user.Location,
                UserPhoneNumber = user.PhoneNumber,
                Items = paginatedItems,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchFilter = search,
                TotalCount = totalItems,
                StartIndex = startIndex,
                EndIndex = endIndex
            };

            return View(model);
        }



        // [HttpPost]
        // [Route("Cart/DeleteItem/{id}")]
        // public async Task<IActionResult> DeleteItem(int id)
        // {
        //     var cartItem = await _context.CartItems.FindAsync(id);
        //     if (cartItem == null)
        //     {
        //         TempData["ErrorMessage"] = "Item not found.";
        //         return RedirectToAction("CartItem");
        //     }

        //     _context.CartItems.Remove(cartItem);
        //     await _context.SaveChangesAsync();

        //     TempData["SuccessMessage"] = "Item successfully deleted.";
        //     return RedirectToAction("CartItem");
        // }

        public async Task<IActionResult> DeleteItem(int ids)
        {
            var cartItem = await _context.CartItems.FindAsync(ids);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Item not found." + ids;
                return RedirectToAction("CartItem");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item successfully deleted.";
            return RedirectToAction("CartItem");
        }

        public async Task<IActionResult> DeleteFinished(int id)
        {
            var cartItem = await _context.CartFinishedItems.FindAsync(id);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Item not found." + id;
                return RedirectToAction("CartItem");
            }
            _context.CartFinishedItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item successfully deleted.";
            return RedirectToAction("CartFinished");
        }

    }
}
