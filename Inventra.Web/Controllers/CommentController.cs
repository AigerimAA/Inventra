using Inventra.Application.Comments.Commands;
using Inventra.Application.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int inventoryId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });

            await _mediator.Send(new AddCommentCommand(inventoryId, content));
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
