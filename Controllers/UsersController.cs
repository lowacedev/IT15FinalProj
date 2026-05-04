using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITSMS.Controllers
{
    /// <summary>
    /// Users Controller - User management (Admin only)
    /// Authorization: Admin only
    /// </summary>
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Users/Index
        [HttpGet]
        public IActionResult Index(string roleFilter = null, int page = 1, int pageSize = 10)
        {
            var users = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(roleFilter))
                users = users.Where(u => u.Role.RoleName == roleFilter);

            // Get total count before pagination
            var totalCount = users.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Ensure page is valid
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Apply pagination
            var userList = users.OrderBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass pagination info via ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewData["RoleFilter"] = roleFilter;
            return View(userList);
        }

        // GET: Users/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Users/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Roles"] = _context.Roles.ToList();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password, string confirmPassword)
        {
            // Clear model state for PasswordHash since we handle it separately
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");

            // Validate password
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "Password is required.");
            }
            else if (password.Length < 8)
            {
                ModelState.AddModelError("password", "Password must be at least 8 characters.");
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Passwords do not match.");
            }

            if (_context.Users.Any(u => u.Username == user.Username || u.Email == user.Email))
            {
                ModelState.AddModelError("", "Username or email already exists.");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"User {user.Username} created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Roles"] = _context.Roles.ToList();
            return View(user);
        }

        // GET: Users/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            ViewData["Roles"] = _context.Roles.ToList();
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string newPassword = null)
        {
            if (id != user.UserId)
                return NotFound();

            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (existingUser == null)
                return NotFound();

            try
            {
                existingUser.Email = user.Email;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;
                existingUser.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(newPassword))
                    existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, newPassword);

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Details), new { id = existingUser.UserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating user: {ex.Message}");
            }

            ViewData["Roles"] = _context.Roles.ToList();
            return View(user);
        }

        // GET: Users/DeactivateConfirm/5
        [HttpGet]
        public IActionResult DeactivateConfirm(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            return View("Deactivate", user);
        }

        // POST: Users/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {user.Username} has been deactivated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Reactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {user.Username} has been reactivated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
