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
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(IMediator mediator, ICommentRepository commentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _commentRepository = commentRepository;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int inventoryId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", "Inventory", new { id = inventoryId });

            var userId = _userManager.GetUserId(User)!;
            await _mediator.Send(new AddCommentCommand(inventoryId, userId, content));
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
