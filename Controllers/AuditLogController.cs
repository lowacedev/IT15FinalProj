using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;
using ITSMS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSMS.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AuditLog
        public async Task<IActionResult> Index(string moduleFilter, string actionFilter, int page = 1, int pageSize = 15)
        {
            var logsQuery = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(moduleFilter))
            {
                logsQuery = logsQuery.Where(l => l.Module == moduleFilter);
            }

            if (!string.IsNullOrEmpty(actionFilter))
            {
                logsQuery = logsQuery.Where(l => l.Action == actionFilter);
            }

            var totalCount = await logsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var logs = await logsQuery
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Modules = new SelectList(await _context.AuditLogs.Select(l => l.Module).Distinct().ToListAsync(), moduleFilter);
            ViewBag.Actions = new SelectList(await _context.AuditLogs.Select(l => l.Action).Distinct().ToListAsync(), actionFilter);
            ViewBag.SelectedModule = moduleFilter;
            ViewBag.SelectedAction = actionFilter;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(logs);
        }
    }
}
