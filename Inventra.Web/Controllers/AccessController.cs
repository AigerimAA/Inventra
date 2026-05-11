using Inventra.Application.Access.Commands.AddAccess;
using Inventra.Application.Access.Commands.RemoveAccess;
using Inventra.Application.Access.Queries.GetInventoryUsers;
using Inventra.Application.Access.Queries.SearchUsers;
using Inventra.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    [Authorize]
    public class AccessController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryPermissionService _permissionService;

        public AccessController(IMediator mediator, ICurrentUserService currentUserService, IInventoryPermissionService permissionService)
        { 
            _mediator = mediator; 
            _currentUserService = currentUserService; 
            _permissionService = permissionService; 
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(int inventoryId)
        {
            try
            {
                var users = await _mediator.Send(new GetInventoryUsersQuery(inventoryId));
                return Json(users);
            }
            catch (UnauthorizedAccessException) 
            { 
                return Forbid(); 
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(new List<object>());
            var users = await _mediator.Send(new SearchUsersQuery(query));
            return Json(users.Select(u => new { id = u.Id, userName = u.UserName, email = u.Email }));
        }        

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AccessRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new AddAccessCommand(request.InventoryId, request.UserId), cancellationToken);
                return Ok(new { success = true, userName = result.UserName, email = result.Email });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] AccessRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new RemoveAccessCommand(request.InventoryId, request.UserId), cancellationToken);
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
    public class AccessRequest
    {
        public int InventoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}

