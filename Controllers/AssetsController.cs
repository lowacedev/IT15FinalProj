using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITSMS.Services.AuditService _auditService;

        public AssetsController(ApplicationDbContext context, ITSMS.Services.AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Assets
        public async Task<IActionResult> Index(string searchString, string category, AssetStatus? status, int page = 1, int pageSize = 10)
        {
            var assets = from a in _context.Assets
                         select a;

            if (!string.IsNullOrEmpty(searchString))
            {
                assets = assets.Where(s => s.AssetTag.Contains(searchString) || s.AssetName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(category))
            {
                assets = assets.Where(x => x.Category == category);
            }

            if (status.HasValue)
            {
                assets = assets.Where(x => x.Status == status.Value);
            }

            // Get total count before pagination
            var totalCount = await assets.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Ensure page is valid
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            ViewBag.Categories = new SelectList(await _context.Assets.Select(a => a.Category).Distinct().ToListAsync());
            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            
            // Include Current Assignment Info (if working and assigned)
            var assetList = await assets
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(a => a.Assignments.Where(asgn => asgn.ReturnedDate == null))
                    .ThenInclude(asgn => asgn.Employee)
                        .ThenInclude(e => e.User)
                .ToListAsync();

            return View(assetList);
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .Include(a => a.Assignments)
                    .ThenInclude(asgn => asgn.Employee)
                        .ThenInclude(e => e.User)
                .Include(a => a.ServiceRequests)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AssetTag,AssetName,Category,Status,PurchaseDate,WarrantyExpiry")] Asset asset)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }
            return View(asset);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssetTag,AssetName,Category,Status,PurchaseDate,WarrantyExpiry")] Asset asset)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.Id))
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
            return View(asset);
        }

        // GET: Assets/Assign/5
        public async Task<IActionResult> Assign(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }
            
            // Check if already assigned
            var activeAssignment = await _context.AssetAssignments
                .FirstOrDefaultAsync(a => a.AssetId == id && a.ReturnedDate == null);
                
            if (activeAssignment != null)
            {
                TempData["ErrorMessage"] = "Asset is already assigned to an employee.";
                return RedirectToAction(nameof(Details), new { id = asset.Id });
            }

            var activeEmployees = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.User.Role)
                .Where(e => e.Status == EmployeeStatus.Active && e.User.Role.RoleName == "Employee")
                .ToListAsync();
            ViewData["EmployeeId"] = new SelectList(activeEmployees.Select(e => new { e.Id, FullName = e.User != null ? e.User.FullName : $"Employee #{e.Id}" }), "Id", "FullName");
            
            var assignment = new AssetAssignment
            {
                AssetId = asset.Id,
                AssignedDate = DateTime.Now
            };
            
            ViewBag.Asset = asset;
            return View(assignment);
        }

        // POST: Assets/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign([Bind("AssetId,EmployeeId,AssignedDate")] AssetAssignment assignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();

                var assetLog = await _context.Assets.FindAsync(assignment.AssetId);
                var userId = GetCurrentUserId();
                _auditService.Log(userId, "ASSIGN", "Asset", $"Assigned {assetLog.AssetTag} to Employee #{assignment.EmployeeId}");

                return RedirectToAction(nameof(Details), new { id = assignment.AssetId });
            }
            
            var asset = await _context.Assets.FindAsync(assignment.AssetId);
            ViewBag.Asset = asset;
            var activeEmps = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.User.Role)
                .Where(e => e.Status == EmployeeStatus.Active && e.User.Role.RoleName == "Employee")
                .ToListAsync();
            ViewData["EmployeeId"] = new SelectList(activeEmps.Select(e => new { e.Id, FullName = e.User != null ? e.User.FullName : $"Employee #{e.Id}" }), "Id", "FullName", assignment.EmployeeId);
            return View(assignment);
        }

        // POST: Assets/Return/5 (Action to mark asset as returned)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var assignment = await _context.AssetAssignments
                .FirstOrDefaultAsync(a => a.Id == id && a.ReturnedDate == null);

            if (assignment != null)
            {
                assignment.ReturnedDate = DateTime.Now;
                _context.Update(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = assignment?.AssetId });
        }

        // POST: Assets/LogCost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogCost(int assetId, decimal amount, FinanceTransactionType transactionType, string description)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var transaction = new FinanceTransaction
            {
                AssetId = assetId,
                Amount = amount,
                TransactionType = transactionType,
                Description = description,
                TransactionDate = DateTime.Now,
                CreatedByUserId = userId
            };

            _context.FinanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Maintenance cost logged successfully to Finance.";
            return RedirectToAction(nameof(Details), new { id = assetId });
        }

        private bool AssetExists(int id)
        {
            return _context.Assets.Any(e => e.Id == id);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }
    }
}
