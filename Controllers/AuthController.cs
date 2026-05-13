using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ITSMS.Data;
using ITSMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITSMS.Controllers
{
    /// <summary>
    /// Authentication controller for user login and registration
    /// Handles cookie-based authentication and session management
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // If user is already authenticated, redirect them to appropriate dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Reports");
                }
                else if (User.IsInRole("Technician"))
                {
                    return RedirectToAction("Index", "TechnicianDashboard");
                }
                else
                {
                    return RedirectToAction("Index", "ServiceRequests");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            // ======================== reCAPTCHA VALIDATION ========================
            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(captchaResponse))
            {
                ModelState.AddModelError("", "Please complete the CAPTCHA verification.");
                return View();
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10); // Don't hang forever
                    var secret = _configuration["GoogleReCaptcha:SecretKey"];
                    var verificationUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={captchaResponse}";
                    
                    var response = await client.PostAsync(verificationUrl, null);
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var captchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonString);

                    if (captchaResult == null || !captchaResult.Success)
                    {
                        ModelState.AddModelError("", "CAPTCHA validation failed. Please try again.");
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if you have a logger, otherwise show generic message
                ModelState.AddModelError("", "Unable to verify CAPTCHA at this time. Please try again later.");
                return View();
            }
            // ======================================================================

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username && u.IsActive);

            if (user == null || !_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password).Equals(PasswordVerificationResult.Success))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            // Create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Employee")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return LocalRedirect(returnUrl ?? "/");
        }

        // GET: Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, 
            string firstName, string lastName, string phoneNumber)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            if (_context.Users.Any(u => u.Username == username || u.Email == email))
            {
                ModelState.AddModelError("", "Username or email already exists.");
                return View();
            }

            var newUser = new User
            {
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                RoleId = 3, // Default: Employee role
                IsActive = true
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Auto-create Employee record for the new user
            var maxEmpId = await _context.Employees.MaxAsync(e => (int?)e.Id) ?? 0;
            var employee = new Employee
            {
                UserId = newUser.UserId,
                DepartmentId = 1, // Default to IT department
                EmployeeCode = $"EMP-{(maxEmpId + 1):000}",
                Status = EmployeeStatus.Active
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        // GET: Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Clear any persisted TempData from the session
            TempData.Clear();
            return RedirectToAction("Login");
        }

        // GET: Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }
    }
}
