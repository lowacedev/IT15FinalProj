using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITSMS.Controllers
{
    [Authorize]
    [ApiController]
    [Route("ServiceRequest/{requestId}/Comment")]
    public class TicketCommentController : Controller
    {
        private readonly TicketCommentService _ticketCommentService;

        public TicketCommentController(TicketCommentService ticketCommentService)
        {
            _ticketCommentService = ticketCommentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int requestId, [FromForm] string body, [FromForm] bool isInternal)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    // Fallback to searching by username if NameIdentifier is not an ID
                    // But assuming standard setup where NameIdentifier is UserId
                    return Unauthorized("User ID not found in claims.");
                }

                // Security check: Only Admin, Helpdesk, Technician can post internal notes
                if (isInternal && !(User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Technician")))
                {
                    return Forbid();
                }

                var comment = await _ticketCommentService.AddCommentAsync(requestId, userId, body, isInternal);

                return Json(new
                {
                    commentId = comment.CommentId,
                    authorName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User",
                    body = comment.Body,
                    isInternal = comment.IsInternal,
                    createdAt = comment.CreatedAt.ToString("MMM dd, hh:mm tt")
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
