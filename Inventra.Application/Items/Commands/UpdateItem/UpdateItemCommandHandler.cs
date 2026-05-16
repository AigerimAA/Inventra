using Inventra.Application.Common.Exceptions;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Items.Commands.UpdateItem
{
    public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand>
    {
        private readonly IItemRepository _itemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryPermissionService _permissionService;

        public UpdateItemCommandHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService, IInventoryPermissionService permissionService)
        {
            _itemRepository = itemRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _permissionService = permissionService;
        }

        public async Task Handle(UpdateItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

            if (!await _permissionService.CanWriteAsync(userId, _currentUserService.IsAdmin, request.InventoryId))
                throw new UnauthorizedAccessException("You do not have write access to this inventory");

            var item = await _itemRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException(nameof(Item), request.Id);

            await _itemRepository.SetOriginalVersionAsync(item, request.Version, cancellationToken);
            item.UpdateValues(
                request.CustomString1Value, request.CustomString2Value, request.CustomString3Value,
                request.CustomInt1Value, request.CustomInt2Value, request.CustomInt3Value,
                request.CustomText1Value, request.CustomText2Value, request.CustomText3Value,
                request.CustomBool1Value, request.CustomBool2Value, request.CustomBool3Value,
                request.CustomLink1Value, request.CustomLink2Value, request.CustomLink3Value,
                request.ImageUrl);

            await _itemRepository.UpdateAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
