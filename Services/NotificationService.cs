using Microsoft.AspNetCore.SignalR;
using ITSMS.Hubs;
using System;
using System.Threading.Tasks;

namespace ITSMS.Services
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendCommentNotification(string userId, string requestNumber, string authorName, bool isInternal, string body)
        {
            var payload = new
            {
                requestNumber = requestNumber,
                authorName = authorName,
                isInternal = isInternal,
                body = body,
                timestamp = DateTime.Now.ToString("O")
            };

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveComment", payload);
        }

        public async Task SendStatusNotification(string userId, string requestNumber, string newStatus, string message)
        {
            var payload = new
            {
                requestNumber = requestNumber,
                newStatus = newStatus,
                message = message,
                timestamp = DateTime.Now.ToString("O")
            };

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveStatusUpdate", payload);
        }
    }
}
