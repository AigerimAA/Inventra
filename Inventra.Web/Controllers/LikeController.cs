using Inventra.Application.Likes.Commands;
using Inventra.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;

        public LikeController(IMediator mediator, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int itemId, int inventoryId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _mediator.Send(new ToggleLikeCommand(itemId, userId));
            return RedirectToAction("Details", "Inventory", new { id = inventoryId });
        }
    }
}
