using AutoMapper;
using Inventra.Application.Common.Exceptions;
using Inventra.Application.DTOs;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetInventoryById
{
    public class GetInventoryByIdQueryHandler : IRequestHandler<GetInventoryByIdQuery, InventoryDto>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public GetInventoryByIdQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }
        public async Task<InventoryDto> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException(nameof(Inventory), request.Id);

            return _mapper.Map<InventoryDto>(inventory);
        }
    }
}
