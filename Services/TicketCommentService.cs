using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Services
{
    public class TicketCommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public TicketCommentService(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<TicketComment> AddCommentAsync(int requestId, int authorId, string body, bool isInternal)
        {
            var comment = new TicketComment
            {
                RequestId = requestId,
                AuthorId = authorId,
                Body = body,
                IsInternal = isInternal,
                CreatedAt = DateTime.Now
            };

            _context.TicketComments.Add(comment);
            
            // Add ActivityLog
            var log = new ActivityLog
            {
                Entity = "TicketComment",
                EntityId = requestId,
                Action = "Created",
                UserId = authorId,
                NewValue = isInternal ? "Internal note added." : "Public comment added.",
                OldValue = "",
                IPAddress = ""
            };
            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();

            // Notify via SignalR
            var request = await _context.ServiceRequests
                .Include(r => r.AssignedTechnician)
                .Include(r => r.Requestor)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
                
            var author = await _context.Users.FindAsync(authorId);
            string authorName = author?.FullName ?? "System";

            if (request != null)
            {
                // Always notify assigned technician (if someone is assigned and it's not the author)
                if (request.AssignedTechnicianId.HasValue && request.AssignedTechnicianId.Value != authorId)
                {
                    await _notificationService.SendCommentNotification(
                        request.AssignedTechnicianId.Value.ToString(),
                        request.RequestNumber ?? request.RequestId.ToString(),
                        authorName,
                        isInternal,
                        body);
                }

                // Notify requestor ONLY if it's NOT internal and requestor is not the author
                if (!isInternal && request.RequestorId != authorId)
                {
                    await _notificationService.SendCommentNotification(
                        request.RequestorId.ToString(),
                        request.RequestNumber ?? request.RequestId.ToString(),
                        authorName,
                        isInternal,
                        body);
                }
            }

            return comment;
        }

        public async Task<List<TicketComment>> GetCommentsAsync(int requestId, bool isStaff)
        {
            var query = _context.TicketComments
                .Include(c => c.Author)
                .Where(c => c.RequestId == requestId);

            if (!isStaff)
            {
                query = query.Where(c => !c.IsInternal);
            }

            return await query.OrderBy(c => c.CreatedAt).ToListAsync();
        }
    }
}
