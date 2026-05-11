using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Comments.Queries
{
    public class GetCommentsByInventoryIdQueryHandler
        : IRequestHandler<GetCommentsByInventoryIdQuery, IEnumerable<CommentDto>>
    {
        private readonly ICommentRepository _commentRepository;

        public GetCommentsByInventoryIdQueryHandler(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<CommentDto>> Handle(
            GetCommentsByInventoryIdQuery request, CancellationToken cancellationToken)
        {
            var comments = await _commentRepository.GetByInventoryIdAsync(request.InventoryId);
            return comments.Select(c => new CommentDto
            {
                AuthorName = c.Author?.UserName ?? "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm")
            });
        }
    }
}
