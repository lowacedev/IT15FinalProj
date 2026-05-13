using System;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Log(int userId, string action, string module, string description)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Module = module,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
