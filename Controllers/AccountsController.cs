using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;

namespace PetalOrSomething.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Account.ToListAsync());
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
            if (ModelState.IsValid)
            {
                if (_context.Account.Any(a => a.Email == account.Email))
                {
                    TempData["ErrorMessage"] = "Email already exists.";
                    return View(account);
                }
                account.Password = HashPassword(account.Password);

                _context.Add(account);
                await _context.SaveChangesAsync();

                await _emailService.SendEmailAsync(account.Email, "Sent Verification Code",
                    $"Your verification code is: {account.VerificationCode}");

                TempData["SuccessMessage"] = "Account successfully created.";
                TempData["Email"] = account.Email;
                return RedirectToAction("Verify", "Accounts");
            }
            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmailAvailability([FromBody] EmailRequest emailRequest)
        {
            if (string.IsNullOrEmpty(emailRequest?.Email))
            {
                return Json("Email is empty.");
            }
            var accounts = await _context.Account.ToListAsync();
            foreach (var account in accounts)
            {
                if (account.Email == emailRequest.Email)
                {
                    return Json(false);
                }
            }
            return Json(true);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (ModelState.IsValid)
            {
                if (Email == "admin@gmail.com" && Password == "admin")
                {
                    HttpContext.Session.SetString("UserId", "admin");
                    HttpContext.Session.SetString("UserName", "ADMINISTRATOR");
                    ViewData["UserId"] = "admin";
                    return RedirectToAction("Index", "Home");
                }

                var account = await _context.Account
                    .FirstOrDefaultAsync(a => a.Email == Email);

                if (account == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Email not found.");
                    return View();
                }

                if (!BCrypt.Net.BCrypt.Verify(Password, account.Password))
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Incorrect password.");
                    return View();
                }

                if (!account.IsVerified)
                {
                    TempData["Email"] = account.Email;
                    return RedirectToAction("Verify", "Accounts");
                }

                HttpContext.Session.SetString("UserId", account.Id.ToString());
                HttpContext.Session.SetString("FirstName", account.FirstName ?? "");
                HttpContext.Session.SetString("MiddleName", account.MiddleName ?? "");
                HttpContext.Session.SetString("LastName", account.LastName ?? "");
                HttpContext.Session.SetString("Email", account.Email ?? "");
                HttpContext.Session.SetString("PhoneNumber", account.PhoneNumber ?? "");
                HttpContext.Session.SetString("Location", account.Location ?? "");

                ViewData["UserId"] = account.Id.ToString();

                TempData["SuccessMessage"] = "Login successful.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Verify()
        {
            string? email = TempData["Email"]?.ToString();
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Accounts");
            }

            ViewData["Email"] = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string Email, string VerificationCode)
        {
            var account = await _context.Account.FirstOrDefaultAsync(a => a.Email == Email);
            if (account == null)
            {
                return BadRequest("Account not found.");
            }

            account.IsVerified = true;
            _context.Update(account);
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = account.Id,
                Title = "Welcome to Our Platform!",
                Message = "Your account has been successfully verified. Welcome aboard, and thank you for joining us!",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your account has been verified. Please log in.";
            return RedirectToAction("Login", "Accounts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendCode(string Email)
        {
            var account = await _context.Account.FirstOrDefaultAsync(a => a.Email == Email);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Email not found.";
                return RedirectToAction("Verify");
            }

            string verificationCode = "123456";
            await _emailService.SendEmailAsync(account.Email, "Resend Verification Code",
                $"Your verification code is: {verificationCode}");

            TempData["SuccessMessage"] = "Verification code has been resent.";
            TempData["Email"] = account.Email;
            return RedirectToAction("Verify");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,Email,Password,PhoneNumber,Location")] Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                _context.Account.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Account.Any(e => e.Id == id);
        }
    }
}
