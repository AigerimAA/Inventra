using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Comments.Commands
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public AddCommentCommandHandler(ICommentRepository commentRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _commentRepository = commentRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

            var comment = new Comment
            {
                InventoryId = request.InventoryId,
                AuthorId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };
            await _commentRepository.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
