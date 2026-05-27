using Inventra.Application.Common.Exceptions;
using Inventra.Application.Interfaces;
using Inventra.Application.Inventories.Queries.GetInventoryById;
using Inventra.Application.Items.Commands.CreateItem;
using Inventra.Application.Items.Commands.DeleteItem;
using Inventra.Application.Items.Commands.UpdateItem;
using Inventra.Application.Items.Queries.GetItemById;
using Inventra.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Inventra.Web.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IInventoryPermissionService _permissionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICloudStorageService _cloudStorageService;

        public ItemController(IMediator mediator, IInventoryPermissionService permissionService, ICurrentUserService currentUserService, ICloudStorageService cloudStorageService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
            _currentUserService = currentUserService;
            _cloudStorageService = cloudStorageService;
        }
        public async Task<IActionResult> Create(int inventoryId)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Forbid();

            if (!await _permissionService.CanWriteAsync(userId, _currentUserService.IsAdmin, inventoryId))
                return Forbid();

            var inventory = await _mediator.Send(new GetInventoryByIdQuery(inventoryId));
            if (inventory == null) return NotFound();

            ViewBag.Inventory = inventory;
            return View(new CreateItemCommand { InventoryId = inventoryId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateItemCommand command)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Forbid();

            if (!await _permissionService.CanWriteAsync(userId, _currentUserService.IsAdmin, command.InventoryId))
                return Forbid();

            if (!ModelState.IsValid)
            {
                var inv = await _mediator.Send(
                    new GetInventoryByIdQuery(command.InventoryId));
                ViewBag.Inventory = inv;
                return View(command);
            }
            await _mediator.Send(command);
            return RedirectToAction("Details", "Inventory", new { id = command.InventoryId });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _mediator.Send(new GetItemByIdQuery(id));
            if (item == null) return NotFound();

            var inventory = await _mediator.Send(new GetInventoryByIdQuery(item.InventoryId));
            ViewBag.Inventory = inventory;

            var userId = _currentUserService.UserId;
            if (userId != null)
            {
                ViewBag.HasWriteAccess = await _permissionService.CanWriteAsync(userId, _currentUserService.IsAdmin, item.InventoryId);
            }
            else
            {
                ViewBag.HasWriteAccess = false;
            }

            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Forbid();

            var item = await _mediator.Send(new GetItemByIdQuery(id));
            if (item == null) return NotFound();

            if (!await _permissionService.CanWriteAsync(userId, _currentUserService.IsAdmin, item.InventoryId))
                return Forbid();

            var inventory = await _mediator.Send(new GetInventoryByIdQuery(item.InventoryId));

            var model = new EditItemViewModel
            {
                Command = new UpdateItemCommand
                {
                    Id = item.Id,
                    InventoryId = item.InventoryId,
                    ImageUrl = item.ImageUrl,
                    CustomString1Value = item.CustomString1Value,
                    CustomString2Value = item.CustomString2Value,
                    CustomString3Value = item.CustomString3Value,
                    CustomInt1Value = item.CustomInt1Value,
                    CustomInt2Value = item.CustomInt2Value,
                    CustomInt3Value = item.CustomInt3Value,
                    CustomText1Value = item.CustomText1Value,
                    CustomText2Value = item.CustomText2Value,
                    CustomText3Value = item.CustomText3Value,
                    CustomBool1Value = item.CustomBool1Value,
                    CustomBool2Value = item.CustomBool2Value,
                    CustomBool3Value = item.CustomBool3Value,
                    CustomLink1Value = item.CustomLink1Value,
                    CustomLink2Value = item.CustomLink2Value,
                    CustomLink3Value = item.CustomLink3Value
                },
                Inventory = inventory,
                VersionString = Convert.ToBase64String(item.Version)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditItemViewModel model)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Forbid();

            try
            {
                model.Command.Version = Convert.FromBase64String(model.VersionString);
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, "Invalid version format");
            }

            if (!ModelState.IsValid)
            {
                var inv = await _mediator.Send(new GetInventoryByIdQuery(model.Command.InventoryId));
                model.Inventory = inv;
                return View(model);
            }
            var imageUrl = model.Command.ImageUrl;
            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = Request.Form["Command.ImageUrl"].FirstOrDefault();

            var command = new UpdateItemCommand
            {
                Id = model.Command.Id,
                InventoryId = model.Command.InventoryId,
                Version = model.Command.Version,
                ImageUrl = imageUrl,
                CustomString1Value = model.Command.CustomString1Value, CustomString2Value = model.Command.CustomString2Value,
                CustomString3Value = model.Command.CustomString3Value, CustomInt1Value = model.Command.CustomInt1Value,
                CustomInt2Value = model.Command.CustomInt2Value, CustomInt3Value = model.Command.CustomInt3Value,
                CustomText1Value = model.Command.CustomText1Value, CustomText2Value = model.Command.CustomText2Value,
                CustomText3Value = model.Command.CustomText3Value, CustomBool1Value = model.Command.CustomBool1Value,
                CustomBool2Value = model.Command.CustomBool2Value, CustomBool3Value = model.Command.CustomBool3Value,
                CustomLink1Value = model.Command.CustomLink1Value, CustomLink2Value = model.Command.CustomLink2Value,
                CustomLink3Value = model.Command.CustomLink3Value,
            };

            try
            {
                await _mediator.Send(command);
                return RedirectToAction(nameof(Details), new { id = model.Command.Id });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Someone else modified this item. Please reload and try again");
                var inventory = await _mediator.Send(new GetInventoryByIdQuery(model.Command.InventoryId));
                model.Inventory = inventory;
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file" });

            await using var stream = file.OpenReadStream();
            var url = await _cloudStorageService.UploadImageAsync(stream, file.FileName, file.ContentType);
            return Ok(new { url });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int inventoryId)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Forbid();

            if (!await _permissionService.CanWriteAsync(
                    userId, _currentUserService.IsAdmin, inventoryId))
                return Forbid();

            await _mediator.Send(new DeleteItemCommand(id));
            return RedirectToAction("Details", "Inventory", new { id = inventoryId });
        }
    }
}
