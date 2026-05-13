using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITSMS.Services.AuditService _auditService;

        public EmployeesController(ApplicationDbContext context, ITSMS.Services.AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int? departmentId, string searchQuery, int page = 1, int pageSize = 10)
        {
            var employeesQuery = _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                employeesQuery = employeesQuery.Where(e =>
                    (e.User.FirstName + " " + e.User.LastName).Contains(searchQuery) ||
                    e.User.Email.Contains(searchQuery) ||
                    (e.EmployeeCode != null && e.EmployeeCode.Contains(searchQuery)));
            }

            // Get total count before pagination
            var totalCount = await employeesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Ensure page is valid
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Apply pagination
            var employees = await employeesQuery.OrderBy(e => e.User.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass pagination info via ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", departmentId);
            ViewBag.SearchQuery = searchQuery;

            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.AssetAssignments)
                    .ThenInclude(a => a.Asset)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            // Get users who don't already have an Employee record
            var usersWithEmployee = await _context.Employees.Select(e => e.UserId).ToListAsync();
            var availableUsers = await _context.Users
                .Where(u => u.IsActive && !usersWithEmployee.Contains(u.UserId))
                .ToListAsync();

            ViewData["UserId"] = new SelectList(availableUsers, "UserId", "FullName");
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,DepartmentId,Position,EmployeeCode,Status,EmployeeNumber,HireDate,EmploymentStatus,SalaryRate")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Auto-generate EmployeeCode if not provided
                if (string.IsNullOrEmpty(employee.EmployeeCode))
                {
                    var maxId = await _context.Employees.MaxAsync(e => (int?)e.Id) ?? 0;
                    employee.EmployeeCode = $"EMP-{(maxId + 1):000}";
                }

                _context.Add(employee);
                await _context.SaveChangesAsync();

                _auditService.Log(GetCurrentUserId(), "CREATE", "Employee", $"Created employee {employee.EmployeeCode}");

                return RedirectToAction(nameof(Index));
            }

            var usersWithEmployee = await _context.Employees.Select(e => e.UserId).ToListAsync();
            var availableUsers = await _context.Users
                .Where(u => u.IsActive && !usersWithEmployee.Contains(u.UserId))
                .ToListAsync();

            ViewData["UserId"] = new SelectList(availableUsers, "UserId", "FullName", employee.UserId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,DepartmentId,Position,EmployeeCode,Status,EmployeeNumber,HireDate,EmploymentStatus,SalaryRate")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    _auditService.Log(GetCurrentUserId(), "UPDATE", "Employee", $"Updated employee {employee.EmployeeCode}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            return View(employee);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }
    }
}
