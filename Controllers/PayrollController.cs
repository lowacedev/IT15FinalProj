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
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payroll
        public async Task<IActionResult> Index()
        {
            var payrolls = _context.Payrolls.Include(p => p.Employee).ThenInclude(e => e.User);
            return View(await payrolls.ToListAsync());
        }

        // GET: Payroll/ExportMonthlySummaryPdf
        [HttpGet]
        public async Task<IActionResult> ExportMonthlySummaryPdf()
        {
            var summary = await _context.Payrolls
                .Include(p => p.Employee).ThenInclude(e => e.User)
                .OrderByDescending(p => p.PayrollMonth)
                .ToListAsync();

            ViewData["Title"] = "Monthly Payroll Summary";
            ViewData["GeneratedBy"] = User.Identity?.Name ?? "Admin User";

            return new ViewAsPdf("MonthlySummaryPdf", summary)
            {
                FileName = $"PayrollSummary_{DateTime.Now:yyyyMMdd}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                ViewData = this.ViewData
            };
        }

        // GET: Payroll/ExportEmployeeHistoryPdf/5
        [HttpGet]
        public async Task<IActionResult> ExportEmployeeHistoryPdf(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            var history = await _context.Payrolls
                .Where(p => p.EmployeeId == id)
                .OrderByDescending(p => p.PayrollMonth)
                .ToListAsync();

            ViewBag.Employee = employee;

            ViewData["Title"] = $"Payroll History - {employee.User?.FullName}";
            ViewData["GeneratedBy"] = User.Identity?.Name ?? "Admin User";

            return new ViewAsPdf("EmployeeHistoryPdf", history)
            {
                FileName = $"PayrollHistory_{employee.EmployeeCode}_{DateTime.Now:yyyyMMdd}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                ViewData = this.ViewData
            };
        }

        // GET: Payroll/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (payroll == null) return NotFound();

            return View(payroll);
        }

        // GET: Payroll/Create
        public IActionResult Create()
        {
            var employees = _context.Employees
                .Include(e => e.User)
                .Select(e => new {
                    Id = e.Id,
                    FullName = e.EmployeeNumber + " - " + (e.User != null ? e.User.FirstName + " " + e.User.LastName : "Unknown")
                });
                
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName");
            return View();
        }

        // POST: Payroll/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,PayrollMonth,BasicSalary,Allowance,Deduction,OvertimePay,PayrollStatus")] Payroll payroll)
        {
            if (ModelState.IsValid)
            {
                payroll.NetSalary = payroll.BasicSalary + payroll.Allowance + payroll.OvertimePay - payroll.Deduction;
                _context.Add(payroll);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var employees = _context.Employees
                .Include(e => e.User)
                .Select(e => new {
                    Id = e.Id,
                    FullName = e.EmployeeNumber + " - " + (e.User != null ? e.User.FirstName + " " + e.User.LastName : "Unknown")
                });
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);
            return View(payroll);
        }

        // GET: Payroll/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null) return NotFound();

            var employees = _context.Employees
                .Include(e => e.User)
                .Select(e => new {
                    Id = e.Id,
                    FullName = e.EmployeeNumber + " - " + (e.User != null ? e.User.FirstName + " " + e.User.LastName : "Unknown")
                });
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);
            return View(payroll);
        }

        // POST: Payroll/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,PayrollMonth,BasicSalary,Allowance,Deduction,OvertimePay,PayrollStatus")] Payroll payroll)
        {
            if (id != payroll.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    payroll.NetSalary = payroll.BasicSalary + payroll.Allowance + payroll.OvertimePay - payroll.Deduction;
                    _context.Update(payroll);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayrollExists(payroll.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var employees = _context.Employees
                .Include(e => e.User)
                .Select(e => new {
                    Id = e.Id,
                    FullName = e.EmployeeNumber + " - " + (e.User != null ? e.User.FirstName + " " + e.User.LastName : "Unknown")
                });
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);
            return View(payroll);
        }

        // GET: Payroll/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (payroll == null) return NotFound();

            return View(payroll);
        }

        // POST: Payroll/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll != null)
            {
                _context.Payrolls.Remove(payroll);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PayrollExists(int id)
        {
            return _context.Payrolls.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalaryRate(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return Json(new { salaryRate = employee.SalaryRate });
        }
    }
}
