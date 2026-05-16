using Inventra.Application.DTOs;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Enums;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.CustomId.Commands
{
    public class SaveCustomIdFormatCommandHandler : IRequestHandler<SaveCustomIdFormatCommand>
    {
        private readonly ICustomIdRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryPermissionService _permissionService;
        public SaveCustomIdFormatCommandHandler(ICustomIdRepository repository, IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService, IInventoryPermissionService permissionService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _permissionService = permissionService;
        }
        public async Task Handle(SaveCustomIdFormatCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
            if (!await _permissionService.CanManageAsync(userId, _currentUserService.IsAdmin, request.Format.InventoryId))
                throw new UnauthorizedAccessException("Only the inventory owner or an admin can edit custom ID format");

            var elements = (request.Format.Elements ?? Enumerable.Empty<CustomIdElementDto>())
                .Select((e, i) =>
                {
                    if (!Enum.TryParse<CustomIdElementType>(e.ElementType, ignoreCase: true, out var elementType))
                        throw new ArgumentException($"Invalid element type: {e.ElementType}");
                    return new CustomIdElement
                    {
                        ElementType = elementType,
                        FormatString = e.FormatString,
                        FixedValue = e.FixedValue,
                        SortOrder = i
                    };
                }).ToList();
            var format = new CustomIdFormat { InventoryId = request.Format.InventoryId, UpdatedAt = DateTime.UtcNow, Elements = elements };

            await _repository.SaveAsync(format, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
