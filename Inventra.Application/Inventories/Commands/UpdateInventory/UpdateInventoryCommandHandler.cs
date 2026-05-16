using Inventra.Application.Common.Exceptions;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Commands.UpdateInventory
{
    public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, byte[]>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryPermissionService _permissionService;

        public UpdateInventoryCommandHandler(IInventoryRepository inventoryRepository, ITagRepository tagRepository,
            IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IInventoryPermissionService permissionService)
        {
            _inventoryRepository = inventoryRepository;
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _permissionService = permissionService;
        }
        public async Task<byte[]> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated");

            var inventory = await _inventoryRepository.GetByIdAsync(request.Id)
                ?? throw new NotFoundException(nameof(Inventory), request.Id);

            if (!inventory.CanEdit(userId, _currentUserService.IsAdmin))
                throw new UnauthorizedAccessException("Only the inventory owner or an admin can edit this inventory");

            _inventoryRepository.SetOriginalVersion(inventory, request.Version);

            inventory.UpdateDetails(request.Title, request.Description, request.ImageUrl,request.IsPublic, request.CategoryId);

            inventory.UpdateFields(
                request.CustomString1Name, request.CustomString1Shown,
                request.CustomString2Name, request.CustomString2Shown,
                request.CustomString3Name, request.CustomString3Shown,
                request.CustomInt1Name, request.CustomInt1Shown,
                request.CustomInt2Name, request.CustomInt2Shown,
                request.CustomInt3Name, request.CustomInt3Shown,
                request.CustomText1Name, request.CustomText1Shown,
                request.CustomText2Name, request.CustomText2Shown,
                request.CustomText3Name, request.CustomText3Shown,
                request.CustomBool1Name, request.CustomBool1Shown,
                request.CustomBool2Name, request.CustomBool2Shown,
                request.CustomBool3Name, request.CustomBool3Shown,
                request.CustomLink1Name, request.CustomLink1Shown,
                request.CustomLink2Name, request.CustomLink2Shown,
                request.CustomLink3Name, request.CustomLink3Shown);

            var tags = new List<Tag>();
            foreach (var tagName in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalized = tagName.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(normalized)) continue;
                tags.Add(await _tagRepository.GetOrCreateAsync(normalized));
            }
            inventory.ReplaceTags(tags);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return inventory.Version;
        }
    }
}
