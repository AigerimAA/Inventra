using Inventra.Application.Comments.Commands;
using Inventra.Application.Comments.Queries;
using Inventra.Web.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Inventra.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<ChatHub> _hubContext;

        public CommentController(IMediator mediator, IHubContext<ChatHub> hubContext)
        {
            _mediator = mediator;
            _hubContext = hubContext;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int inventoryId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });

            await _mediator.Send(new AddCommentCommand(inventoryId, content));

            var userName = User.Identity?.Name ?? "Anonymous";
            var timestamp = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm");
            await _hubContext.Clients.Group(inventoryId.ToString())
                .SendAsync("ReceiveMessage", userName, content, timestamp);

            return RedirectToAction("Details", "Inventory", new { id = inventoryId });
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int inventoryId)
        {
            var comments = await _mediator.Send(new GetCommentsByInventoryIdQuery(inventoryId));
            return Json(comments);
        }
    }
}
