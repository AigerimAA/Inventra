using Inventra.Application.DTOs;
using Inventra.Application.Interfaces;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Access.Commands.AddAccess
{
    public class AddAccessCommandHandler : IRequestHandler<AddAccessCommand, AccessUserDto>
    {
        private readonly IAccessRepository _accessRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryPermissionService _permissionService;
        public AddAccessCommandHandler(IAccessRepository accessRepository, ICurrentUserService currentUserService, IInventoryPermissionService permissionService)
        { 
            _accessRepository = accessRepository; 
            _currentUserService = currentUserService; 
            _permissionService = permissionService; 
        }

        public async Task<AccessUserDto> Handle(AddAccessCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
            if (!await _permissionService.CanManageAsync(userId, _currentUserService.IsAdmin, request.InventoryId))
                throw new UnauthorizedAccessException();
            if (request.TargetUserId == userId)
                throw new InvalidOperationException("You already have access as the owner");

            var access = await _accessRepository.AddAccessAsync(request.InventoryId, request.TargetUserId, cancellationToken);
            return new AccessUserDto { Id = access.User.Id, UserName = access.User.UserName!, Email = access.User.Email! };
        }
    }
}

