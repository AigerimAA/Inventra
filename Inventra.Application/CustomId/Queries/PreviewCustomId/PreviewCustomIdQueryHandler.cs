using Inventra.Application.Interfaces;
using MediatR;

namespace Inventra.Application.CustomId.Queries.PreviewCustomId
{
    public class PreviewCustomIdQueryHandler : IRequestHandler<PreviewCustomIdQuery, string>
    {
        private readonly ICustomIdGenerator _customIdGenerator;

        public PreviewCustomIdQueryHandler(ICustomIdGenerator customIdGenerator)
        {
            _customIdGenerator = customIdGenerator;
        }

        public async Task<string> Handle(PreviewCustomIdQuery request, CancellationToken cancellationToken)
        {
            return await _customIdGenerator.GenerateAsync(request.InventoryId, cancellationToken);
        }
    }
}
