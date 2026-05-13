using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;
using ITSMS.Models;
using System.Linq;
using System.Linq;
using System.Threading.Tasks;
using Rotativa.AspNetCore;

namespace ITSMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class FinanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Finance/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Dashboard widgets calculations
            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            
            var totalMonthlyExpenses = await _context.FinanceTransactions
                .Where(t => t.TransactionDate.Month == DateTime.Now.Month && t.TransactionDate.Year == DateTime.Now.Year)
                .SumAsync(t => t.Amount);

            var totalPayrollExpenses = await _context.Payrolls
                .Where(p => p.PayrollMonth == currentMonth)
                .SumAsync(p => p.NetSalary);

            var pendingPayrollCount = await _context.Payrolls
                .Where(p => p.PayrollStatus == PayrollStatus.Pending)
                .CountAsync();

            var topDepartment = await _context.FinanceTransactions
                .Include(t => t.Department)
                .Where(t => t.DepartmentId != null && t.TransactionDate.Month == DateTime.Now.Month && t.TransactionDate.Year == DateTime.Now.Year)
                .GroupBy(t => t.Department.Name)
                .Select(g => new { DepartmentName = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(g => g.Total)
                .FirstOrDefaultAsync();

            var recentTransactions = await _context.FinanceTransactions
                .Include(t => t.Department)
                .Include(t => t.Asset)
                .Include(t => t.ServiceRequest)
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalMonthlyExpenses = totalMonthlyExpenses;
            ViewBag.TotalPayrollExpenses = totalPayrollExpenses;
            ViewBag.PendingPayrollCount = pendingPayrollCount;
            ViewBag.TopDepartmentName = topDepartment?.DepartmentName ?? "N/A";
            ViewBag.TopDepartmentTotal = topDepartment?.Total ?? 0m;
            
            return View(recentTransactions);
        }

        // GET: Finance/Transactions
        public async Task<IActionResult> Transactions()
        {
            var transactions = _context.FinanceTransactions
                .Include(t => t.Department)
                .Include(t => t.Asset)
                .Include(t => t.ServiceRequest)
                .Include(t => t.CreatedByUser)
                .OrderByDescending(t => t.TransactionDate);
                
            return View(await transactions.ToListAsync());
        }

        // GET: Finance/Reports
        public async Task<IActionResult> Reports()
        {
            // Get expenses by department
            var expensesByDept = await _context.FinanceTransactions
                .Include(t => t.Department)
                .Where(t => t.DepartmentId != null)
                .GroupBy(t => t.Department.Name)
                .Select(g => new { DepartmentName = g.Key, Total = g.Sum(t => t.Amount) })
                .ToListAsync();

            ViewBag.ExpensesLabels = System.Text.Json.JsonSerializer.Serialize(expensesByDept.Select(e => e.DepartmentName));
            ViewBag.ExpensesData = System.Text.Json.JsonSerializer.Serialize(expensesByDept.Select(e => e.Total));

            // Get monthly payroll summary (only Paid)
            var payrollSummary = await _context.Payrolls
                .Where(p => p.PayrollStatus == PayrollStatus.Paid)
                .GroupBy(p => p.PayrollMonth)
                .Select(g => new { Month = g.Key, Total = g.Sum(p => p.NetSalary) })
                .ToListAsync();

            var sortedPayroll = payrollSummary
                .Select(p => new {
                    MonthName = p.Month, 
                    Total = p.Total,
                    Date = DateTime.TryParse(p.Month, out var dt) ? dt : DateTime.MinValue 
                })
                .OrderBy(p => p.Date)
                .ToList();

            ViewBag.PayrollLabels = System.Text.Json.JsonSerializer.Serialize(sortedPayroll.Select(p => p.MonthName));
            ViewBag.PayrollData = System.Text.Json.JsonSerializer.Serialize(sortedPayroll.Select(p => p.Total));

            return View();
        }

        // GET: Finance/ExportFinanceReportPdf
        public async Task<IActionResult> ExportFinanceReportPdf()
        {
            var expensesByDept = await _context.FinanceTransactions
                .Include(t => t.Department)
                .Where(t => t.DepartmentId != null)
                .GroupBy(t => t.Department.Name)
                .Select(g => new { DepartmentName = g.Key, Total = g.Sum(t => t.Amount) })
                .ToListAsync();

            var payrollSummary = await _context.Payrolls
                .Where(p => p.PayrollStatus == PayrollStatus.Paid)
                .GroupBy(p => p.PayrollMonth)
                .Select(g => new { Month = g.Key, Total = g.Sum(p => p.NetSalary) })
                .ToListAsync();

            var sortedPayroll = payrollSummary
                .Select(p => new {
                    MonthName = p.Month, 
                    Total = p.Total,
                    Date = DateTime.TryParse(p.Month, out var dt) ? dt : DateTime.MinValue 
                })
                .OrderBy(p => p.Date)
                .ToList();

            ViewBag.ExpensesByDept = expensesByDept;
            ViewBag.PayrollSummary = sortedPayroll;
            
            ViewData["Title"] = "Financial Report";
            ViewData["GeneratedBy"] = User.Identity?.Name ?? "Admin User";

            return new ViewAsPdf("FinanceReportPdf")
            {
                FileName = $"FinanceReport_{DateTime.Now:yyyyMMdd}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                ViewData = this.ViewData
            };
        }

        // GET: Finance/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Finance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentId,TransactionType,Amount,Description,TransactionDate")] FinanceTransaction financeTransaction)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim?.Value, out int userId))
                {
                    financeTransaction.CreatedByUserId = userId;
                }
                else
                {
                    financeTransaction.CreatedByUserId = 1; // Fallback
                }

                _context.Add(financeTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Dashboard));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", financeTransaction.DepartmentId);
            return View(financeTransaction);
        }
    }
}
