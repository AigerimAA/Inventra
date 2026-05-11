using Inventra.Application.Comments.Commands;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    public class CommentController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICommentRepository _commentRepository;

        public CommentController(IMediator mediator, ICommentRepository commentRepository)
        {
            _mediator = mediator;
            _commentRepository = commentRepository;
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
            var comments = await _commentRepository.GetByInventoryIdAsync(inventoryId);
            var result = comments.Select(c => new
            {
                authorName = c.Author?.UserName,
                content = c.Content,
                createdAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm")
            });
            return Json(result);
        }
    }
}
