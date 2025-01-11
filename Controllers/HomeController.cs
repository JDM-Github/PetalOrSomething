using InquiryManagementApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using System.Diagnostics;
using System.Text;


// var orderRequest = new OrdersCreateRequest();
// orderRequest.Prefer("return=representation");
// orderRequest.RequestBody(new OrderRequest
// {
//     CheckoutPaymentIntent = "CAPTURE",
//     ApplicationContext = new ApplicationContext
//     {
//         ReturnUrl = Url.Action("PaymentBalanceSuccess", "Payment", new { userId, paymentId = payment.Id, amount = firstPayment }, Request.Scheme),
//         CancelUrl = Url.Action("PaymentBalanceFailed", "Payment", new { userId, paymentId = payment.Id, amount = firstPayment }, Request.Scheme)
//     },
//     PurchaseUnits = new List<PurchaseUnitRequest>
//             {
//                 new PurchaseUnitRequest
//                 {
//                     AmountWithBreakdown = new AmountWithBreakdown
//                     {
//                         CurrencyCode = "PHP",
//                         Value = firstPayment.ToString("F2")
//                     },
//                     Description = "Balance Fee"
//                 }
//             }
// });

// try
// {
//     var client = new PayPalHttpClient(PayPalConfig.GetEnvironment());
//     var response = await client.Execute(orderRequest);
//     var result = response.Result<Order>();

//     var approvalUrl = result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
//     if (approvalUrl != null)
//     {
//         return Redirect(approvalUrl);
//     }
//     else
//     {
//         TempData["ErrorMessage"] = "Payment approval URL not found.";
//         return RedirectToAction("PaymentHistory");
//     }
// }
// catch (Exception ex)
// {
//     TempData["ErrorMessage"] = $"Error during payment: {ex.Message}";
//     return RedirectToAction("PaymentHistory");
// }

namespace PetalOrSomething.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        public readonly FileUploadService _fileUploadService;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, FileUploadService fileUploadService, HttpClient httpClient)
        {
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            Console.WriteLine($"UserId: {ViewData["UserId"]}");
            _logger.LogInformation($"UserId: {ViewData["UserId"]}");

            if (userId == "admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        public IActionResult EditDesign(string ids, string name, string quantity, string note, string customization)
        {
            ViewData["Ids"] = ids;
            ViewData["Name"] = name;
            ViewData["Quantity"] = quantity;
            ViewData["Note"] = note;
            ViewData["Customization"] = customization;
            return View();
        }

        public IActionResult ProductEdit(string searchQuery = "", string categoryFilter = "all")
        {
            var assets = _context.Assets.Where(c => c.IsAvailable == true).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                assets = assets.Where(a => a.Name.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "all")
            {
                assets = assets.Where(a => a.Category == categoryFilter);
            }

            var viewModel = new ProductEditViewModel
            {
                SearchQuery = searchQuery,
                CategoryFilter = categoryFilter,
                Assets = assets.ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetAssets(string searchQuery, string categoryFilter)
        {
            var query = _context.Assets.AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(a => a.Name.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "all")
            {
                query = query.Where(a => a.Category == categoryFilter);
            }

            var assets = query.ToList();
            return Json(assets);
        }


        // public IActionResult OrderHere()
        // {
        //     var userId = HttpContext.Session.GetString("UserId");
        //     ViewData["UserId"] = userId;
        //     return View();
        // }
        public async Task<IActionResult> OrderHere(int page = 1, string category = "All")
        {
            int pageSize = 10;
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;

            var query = _context.FlowerInventories.Where(c => c.IsAvailable == true).AsQueryable();
            if (category != "All")
            {
                query = query.Where(f => f.Category == category);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var products = await query
                .Include(f => f.Stocks)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var productsWithStock = products
                .Where(f => f.Stocks
                    .Where(s => s.ExpiryDate >= DateTime.Now)
                    .Sum(s => s.Quantity) > 0)
                .ToList();
            Console.WriteLine(productsWithStock);
            var viewModel = new OrderHereViewModel
            {
                Products = productsWithStock,
                CurrentPage = page,
                TotalPages = totalPages,
                SelectedCategory = category
            };

            return View(viewModel);
        }

        public async Task<ActionResult> Notification(int page = 1)
        {
            int pageSize = 10;
            var query = _context.Notifications.AsQueryable();

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
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var notification = new NotificationView
            {
                Notifications = notifications,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex
            };
            return View(notification);
        }

        public IActionResult View3D()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult Privacy()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult About()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        public IActionResult ProductView(int id)
        {
            var product = _context.FlowerInventories
                                .Include(p => p.Stocks)
                                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewData["UserId"] = userId;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GetProducts()
        {
            var products = _context.FlowerInventories
                .Select(f => new
                {
                    f.TotalStock,
                    f.Id,
                    f.Name,
                    f.Model3DLink,
                    f.Price,
                    Rating = 4.5
                }).ToList();

            return Json(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(int productId, int quantity, string note, decimal price)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var totalAmount = price * quantity;

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = totalAmount * 100,
                        currency = "PHP",
                        description = "Payment for product",
                        remarks = "Order a product"
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("sk_test_PoK58FtMrQaHHc2EyguAKYwj")));
                var response = await _httpClient.PostAsync(
                    "https://api.paymongo.com/v1/links",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(responseBody);

                    string checkoutUrl = responseData.data.attributes.checkout_url;
                    string referenceNumber = responseData.data.attributes.reference_number;



                    // Update Cart (You can fetch cart details based on userId and remove the ordered product)
                    // Assuming you have some method to update your cart here

                    // Optionally, store the order in the database with the reference number and order details
                    // SaveOrder(userId, referenceNumber, productId, quantity);

                    // Redirect to PayMongo checkout page
                    return Redirect(checkoutUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create payment link. Please try again.");
                    TempData["ErrorMessage"] = "Failed to create payment link. Please try again.";
                    return View("ProductView");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating payment: {ex.Message}");
                TempData["ErrorMessage"] = $"Error creating payment: {ex.Message}";
                return View("ProductView");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int productId, int quantity, string note)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = await _context.Account.FirstOrDefaultAsync(c => c.Id == int.Parse(userId!));
            if (userId == null)
            {
                TempData["ErrorMessage"] = "User not logged in.";
                return RedirectToAction("Register", "Account");
            }
            if (string.IsNullOrEmpty(user!.Location)
            || string.IsNullOrEmpty(user.PhoneNumber))
            {
                TempData["ErrorMessage"] = "Please update your account details.";
                return RedirectToAction("ProductView", "Home");
            }

            if (string.IsNullOrWhiteSpace(note))
            {
                note = "No note provided.";
            }
            var cartFinished = new CartFinished(
                int.Parse(userId), productId, quantity, note
            );
            _context.CartFinishedItems.Add(cartFinished);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Product added to cart successfully!";
            return RedirectToAction("CartFinished", "Cart");
        }


        [HttpPost]
        public async Task<IActionResult> EditCustomItem(string ids, string customization, string productName, int quantity, string note, double total, IFormFile glbFile)
        {
            if (string.IsNullOrEmpty(ids))
            {
                TempData["ErrorMessage"] = "Invalid ID provided.";
                return RedirectToAction("ProductEdit");
            }

            if (!int.TryParse(ids, out int itemId))
            {
                TempData["ErrorMessage"] = "Invalid ID format.";
                return RedirectToAction("ProductEdit");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "User not logged in.";
                return RedirectToAction("ProductEdit");
            }

            if (string.IsNullOrWhiteSpace(productName) || quantity <= 0)
            {
                TempData["Customization"] = customization;
                TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                return RedirectToAction("ProductEdit");
            }

            var cartItem = await _context.CartItems.FindAsync(itemId);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Item not found.";
                return RedirectToAction("ProductEdit");
            }

            cartItem.CustomizationJson = customization;
            cartItem.ProductName = productName;
            cartItem.Quantity = quantity;
            cartItem.TotalPrice = total;
            cartItem.Note = note;

            if (glbFile != null)
            {
                var model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(glbFile);
                cartItem.Model3DLink = model3DLink;
            }

            try
            {
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item successfully updated in cart.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "There was an error updating the item in your cart.";
                return RedirectToAction("ProductEdit");
            }

            return RedirectToAction("CartItem", "Cart");
        }

        public async Task<IActionResult> CreateOrUpdateAccount(string firstName, string middleName, string lastName, string email, string phoneNumber, string location)
        {
            if (ModelState.IsValid)
            {
                if (phoneNumber == "")
                {
                    TempData["ErrorMessage"] = "Phone number is required.";
                    return RedirectToAction("Index");
                }
                if (location == "")
                {
                    TempData["ErrorMessage"] = "Location is required.";
                    return RedirectToAction("Index");
                }
                var currentAccount = await _context.Account
                    .FirstOrDefaultAsync(a => a.Email == email);

                if (currentAccount == null)
                {
                    TempData["ErrorMessage"] ="Account not found."; 
                    return RedirectToAction("Index");
                }
                else
                {
                    currentAccount.FirstName = firstName;
                    currentAccount.MiddleName = middleName;
                    currentAccount.LastName = lastName;
                    currentAccount.PhoneNumber = phoneNumber;
                    currentAccount.Location = location;

                    HttpContext.Session.SetString("FirstName", firstName);
                    HttpContext.Session.SetString("MiddleName", middleName);
                    HttpContext.Session.SetString("LastName", lastName);
                    HttpContext.Session.SetString("PhoneNumber", phoneNumber);
                    HttpContext.Session.SetString("Location", location);

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Account successfully updated.";
                    return RedirectToAction("Index");
                }
            }
            TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomItem(string customization, string productName, int quantity, string note, double total, IFormFile glbFile)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var user = await _context.Account.FirstOrDefaultAsync(c => c.Id == int.Parse(userId!));
            if (userId == null)
            {
                TempData["Customization"] = customization;
                TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                return RedirectToAction("ProductEdit");
            }
            if (string.IsNullOrEmpty(user!.Location)
            || string.IsNullOrEmpty(user.PhoneNumber))
            {
                TempData["Customization"] = customization;
                TempData["ErrorMessage"] = "Please update your account details.";
                return RedirectToAction("ProductEdit");
            }
            if (string.IsNullOrWhiteSpace(productName) || quantity <= 0)
            {
                TempData["Customization"] = customization;
                TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                Console.WriteLine("Invalid input. Please check your entries.");
                return RedirectToAction("ProductEdit");
            }

            if (total <= 0)
            {
                TempData["Customization"] = customization;
                TempData["ErrorMessage"] = "Invalid input. Please check your entries.";
                Console.WriteLine("Invalid input. Please check your entries.");
                return RedirectToAction("ProductEdit");
            }

            var model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(glbFile);
            var cartItem = new CartItem
            {
                Model3DLink = model3DLink,
                UserId = int.Parse(userId!),
                CustomizationJson = customization,
                ProductName = productName,
                Quantity = quantity,
                TotalPrice = total,
                Note = note
            };

            try
            {
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item successfully added to cart.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "There was an error adding the item to your cart.";
                TempData["Customization"] = customization;
                return RedirectToAction("ProductEdit");
            }

            return RedirectToAction("CartItem", "Cart");
        }

        // [HttpPost]
        // public IActionResult AddCustomItem(int productId, int quantity, string note)
        // {
        //     var userId = HttpContext.Session.GetString("UserId");
        //     if (userId == null)
        //     {
        //         return RedirectToAction("CartFinished", "Cart");
        //     }

        //     if (string.IsNullOrWhiteSpace(note))
        //     {
        //         note = "No note provided.";
        //     }
        //     var cartFinished = new CartFinished(
        //         int.Parse(userId), productId, quantity, note
        //     );
        //     _context.CartFinishedItems.Add(cartFinished);
        //     _context.SaveChanges();

        //     TempData["SuccessMessage"] = "Product added to cart successfully!";
        //     return RedirectToAction("CartFinished", "Cart");
        // }

        [HttpPost]
        public IActionResult PlaceOrder(int productId, int quantity)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("CartFinished", "Cart");
            }

            var product = _context.FlowerInventories.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return BadRequest("Insufficient stock or invalid product.");
            }

            var totalPrice = product.Price * quantity;
            var order = new Order
            {
                UserId = int.Parse(userId),
                ProductId = productId,
                Quantity = quantity,
                TotalPrice = totalPrice
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }



        // [HttpPost]
        // public IActionResult AddItem(int ProductId, int Quantity)
        // {
        //     Console.WriteLine("--------------------------------------------------------------");
        //     Console.WriteLine(ProductId);
        //     Console.WriteLine("--------------------------------------------------------------");

        //     return Ok(new { message = "Product added to cart successfully!" });

        // var userId = HttpContext.Session.GetString("UserId");
        // if (string.IsNullOrEmpty(userId))
        // {
        //     return Unauthorized();
        // }

        // Console.WriteLine(item);
        // return Ok(new { message = "Product added to cart successfully!" });




        // var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
        // if (cart == null)
        // {
        //     cart = new Cart { UserId = userId };
        //     _context.Carts.Add(cart);
        //     _context.SaveChanges();
        // }

        // var existingItem = _context.CartFinishedItems.FirstOrDefault(i => i.CartId == cart.Id && i.ProductId == item.ProductId);

        // if (existingItem != null)
        // {
        //     existingItem.Quantity += item.Quantity;
        // }
        // else
        // {
        //     item.CartId = cart.Id;
        //     _context.CartFinishedItems.Add(item);
        // }

        //     _context.SaveChanges();
        //     return Ok(new { message = "Product added to cart successfully!" });
        // }

        public IActionResult CartItem()
        {
            var cartItems = _context.CartItems
            .Join(
                _context.Account,
                cart => cart.UserId,
                user => user.Id,
                (cart, user) => new CartItemWithUserViewModel
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    UserFirstName = user.FirstName,
                    UserLastName = user.LastName,
                    UserEmail = user.Email,
                    Model3DLink = cart.Model3DLink,
                    Quantity = cart.Quantity,
                    ProductName = cart.ProductName,
                    ProductPrice = cart.TotalPrice,
                    Customization = cart.CustomizationJson,
                    Note = cart.Note
                })
            .ToList();

            Console.WriteLine(cartItems);
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(IFormFile GLBFile, string Name, int Quantity)
        {
            // var userid = HttpContext.Session.GetString("UserId");
            // var model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(GLBFile);
            // var newCart = new CartItem(model3DLink, int.Parse(userid!), Name, Quantity);
            // _context.CartItems.Add(newCart);
            // await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");

            // var userId = HttpContext.Session.GetString("UserId");
            // if (string.IsNullOrEmpty(userId))
            // {
            //     return RedirectToAction("Index", "Home");
            // }
            // var cart = _context.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
            // string model3DLink = "";
            // Console.WriteLine("TEST1");
            // if (GLBFile != null && GLBFile.Length > 0)
            // {
            //     Console.WriteLine(model3DLink);
            //     model3DLink = await _fileUploadService.UploadFileToCloudinaryAsync(GLBFile);
            // }
            // Console.WriteLine("TEST2");
            // Console.WriteLine(model3DLink);
            // double price = CalculatePrice(orderName);
            // var cartItem = new CartItem
            // {
            //     ProductName = orderName,
            //     Model3DLink = model3DLink,
            //     Price = price,
            //     Quantity = quantity,
            //     CartId = cart!.Id
            // };

            // cart.Items.Add(cartItem);
            // _context.Update(cart);
            // await _context.SaveChangesAsync();
            // Console.Write("GOOD");
            // return RedirectToAction("Index", "Home");
        }

        private double CalculatePrice(string orderName)
        {
            throw new NotImplementedException();
        }
    }
}
