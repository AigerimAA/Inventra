using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Likes.Commands
{
    public class ToggleLikeCommandHandler : IRequestHandler<ToggleLikeCommand>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ToggleLikeCommandHandler(ILikeRepository likeRepository, IUnitOfWork unitOfWork)
        {
            _likeRepository = likeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
        {
            var existing = await _likeRepository.GetByUserAndItemAsync(request.ItemId, request.UserId, cancellationToken);
            if (existing != null)
                await _likeRepository.RemoveAsync(existing, cancellationToken);
            else
                await _likeRepository.AddAsync(new Like(request.ItemId, request.UserId), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
