using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

            transaction.Status = transaction.RemainingBalance > 0 ? "Downpayment" : "Paid";
            transaction.OrderStatus = "Pending";
            transaction.PaymentMethod = "Paymongo";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(_configuration["PayMongo:SecretKey"])));
            var response = await _httpClient.GetAsync(
                "https://api.paymongo.com/v1/checkout_sessions/" + transaction.TransactionId.ToString());

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDeserialized = JsonConvert.DeserializeObject<dynamic>(responseContent);

                transaction.PaymentMethod = ((string)jsonDeserialized!.data!.attributes!.payment_method_used).ToUpper();
            }

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
                return RedirectToAction("CartFinished");
            }

            transaction.Status = transaction.RemainingBalance > 0 ? "Downpayment" : "Paid";
            transaction.OrderStatus = "Pending";
            transaction.PaymentMethod = "Paymongo";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(_configuration["PayMongo:SecretKey"])));
            var response = await _httpClient.GetAsync(
                "https://api.paymongo.com/v1/checkout_sessions/" + transaction.TransactionId.ToString());

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDeserialized = JsonConvert.DeserializeObject<dynamic>(responseContent);

                transaction.PaymentMethod = ((string)jsonDeserialized!.data!.attributes!.payment_method_used).ToUpper();
            }

            var cartItemToUpdate = transaction.Products;
            foreach (var cartItem in cartItemToUpdate)
            {
                if (cartItem.Product != null)
                {
                    cartItem.Product.RemoveStock(cartItem.Quantity);
                }
            }
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
            return RedirectToAction("OrderHistory", "Orders");
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
            transaction.Status = "Cancelled";
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

        public async Task<IActionResult> CreateOrder(
            string willPickup = "Delivery", string paymentType = "Full", int downPayment = 25)
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

            var referenceNumber = Guid.NewGuid().ToString();

            var successUrl = $"{Request.Scheme}://{Request.Host}/Cart/PaymentSuccess?reference={referenceNumber}";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/PaymentCancelled?reference={referenceNumber}";

            double shippingFee = willPickup == "Pickup" ? 0 : 25;
            double totalAmount = cartItems.Sum(c => c.TotalPrice) + shippingFee;

            double calculatedDownPayment = paymentType == "Partial" ? totalAmount * (downPayment / 100.0) : totalAmount;
            double remainingBalance = paymentType == "Partial" ? totalAmount - calculatedDownPayment : 0;
            double downPaymentPercentage = paymentType == "Partial" ? downPayment / 100.0 : 1.0;

            if (remainingBalance < 100 && remainingBalance > 0)
            {
                TempData["ErrorMessage"] = "Sorry but the minimum remaining balance is 100 PHP.";
                return RedirectToAction("CartFinished");
            }

            if (calculatedDownPayment < 100)
            {
                TempData["ErrorMessage"] = "Sorry but the minimum down payment is 100 PHP.";
                return RedirectToAction("CartFinished");
            }

            var adjustedLineItems = cartItems.Select((item, index) => new
            {
                currency = "PHP",
                images = new string[] { item.Product.Model2DLink },
                amount = (int)(item.Product.Price * downPaymentPercentage * 100),
                name = paymentType == "Partial"
                    ? $"{item.Product.Name} - {downPayment}% Down Payment"
                    : item.Product.Name,
                quantity = item.Quantity,
                description = paymentType == "Partial"
                ? "Partial payment for this product"
                : "Full payment for this product"
            }).ToList();

            if (shippingFee > 0)
            {
                adjustedLineItems.Add(new
                {
                    currency = "PHP",
                    images = new string[] { "https://cdn-icons-png.flaticon.com/512/5166/5166991.png" },
                    amount = (int)(shippingFee * 100),
                    name = "Shipping Fee",
                    quantity = 1,
                    description = "Shipping fee for your order"
                });
            }

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
                                // city = "Calamba City",
                                // state = "Laguna",
                                // postal_code = "4027",
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
                        line_items = adjustedLineItems,
                        description = "Payment for selected products",
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

                    var transaction = new TransactionOrder
                    {
                        UserId = user.Id,
                        Products = cartItems,
                        ReferenceNumber = referenceNumber,
                        TransactionId = jsonDeserialized!.data!.id,
                        TotalAmount = totalAmount,
                        ShippingFee = shippingFee,
                        WillPickUp = willPickup == "Pickup",
                        PaymentLink = paymentLink!.ToString(),
                        DownPayment = calculatedDownPayment,
                        RemainingBalance = remainingBalance,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        ExpirationTime = DateTime.UtcNow.AddMinutes(15)
                    };

                    _context.TransactionOrders.Add(transaction);
                    _context.SaveChanges();
                    return Redirect(paymentLink.ToString());
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create payment link.";
                    return RedirectToAction("CartFinished");
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your payment.";
                return RedirectToAction("CartFinished");
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

            if (cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "No items in selected.";
                return RedirectToAction("CartItem");
            }

            var referenceNumber = Guid.NewGuid().ToString();
            var successUrl = $"{Request.Scheme}://{Request.Host}/Cart/PaymentCustomSuccess?reference={referenceNumber}";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/PaymentCustomCancelled?reference={referenceNumber}";

            double shippingFee = willPickup == "Pickup" ? 0 : 25;
            double totalAmount = cartItems.Sum(c => c.TotalPrice * c.Quantity) + shippingFee;

            double calculatedDownPayment = paymentType == "Partial" ? totalAmount * (downPayment / 100.0) : totalAmount;
            double remainingBalance = paymentType == "Partial" ? totalAmount - calculatedDownPayment : 0;
            double downPaymentPercentage = paymentType == "Partial" ? downPayment / 100.0 : 1.0;

            if (remainingBalance < 100 && remainingBalance > 0)
            {
                TempData["ErrorMessage"] = "Sorry but the minimum remaining balance is 100 PHP.";
                return RedirectToAction("CartItem");
            }

            if (calculatedDownPayment < 100)
            {
                TempData["ErrorMessage"] = "Sorry but the minimum down payment is 100 PHP.";
                return RedirectToAction("CartItem");
            }

            var adjustedLineItems = cartItems.Select((item, index) => new
            {
                currency = "PHP",
                images = new string[] { "https://cdn-icons-png.flaticon.com/512/5166/5166991.png" },
                amount = (int)(item.TotalPrice * downPaymentPercentage * 100),
                name = paymentType == "Partial"
                    ? $"{item.ProductName} - {downPayment}% Down Payment"
                    : item.ProductName,
                quantity = item.Quantity,
                description = paymentType == "Partial"
                ? "Partial payment for this product"
                : "Full payment for this product"
            }).ToList();

            if (shippingFee > 0)
            {
                adjustedLineItems.Add(new
                {
                    currency = "PHP",
                    images = new string[] { "https://cdn-icons-png.flaticon.com/512/5166/5166991.png" },
                    amount = (int)(shippingFee * 100),
                    name = "Shipping Fee",
                    quantity = 1,
                    description = "Shipping fee for your order"
                });
            }

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
                        line_items = adjustedLineItems,
                        description = "Payment for selected products",
                        reference_number = referenceNumber,
                        statement_descriptor = "Petal and Planes",
                        success_url = successUrl,
                        cancel_url = cancelUrl,
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            Console.WriteLine("-----------------------------");
            Console.WriteLine(jsonPayload);
            Console.WriteLine("-----------------------------");
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

                    var transaction = new TransactionCustomOrder
                    {
                        UserId = user.Id,
                        Products = cartItems,
                        ReferenceNumber = referenceNumber,
                        TransactionId = jsonDeserialized!.data!.id,
                        TotalAmount = totalAmount,
                        ShippingFee = shippingFee,
                        WillPickUp = willPickup == "Pickup",
                        PaymentLink = paymentLink!.ToString(),
                        DownPayment = calculatedDownPayment,
                        RemainingBalance = remainingBalance,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        ExpirationTime = DateTime.UtcNow.AddMinutes(15)
                    };

                    _context.TransactionCustomOrders.Add(transaction);
                    _context.SaveChanges();
                    return Redirect(paymentLink.ToString());
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create payment link.";
                    return RedirectToAction("CartItem");
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your payment.";
                return RedirectToAction("CartItem");
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
