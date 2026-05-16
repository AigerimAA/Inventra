    using AutoMapper;
    using Inventra.Application.DTOs;
    using Inventra.Application.Interfaces;
    using Inventra.Domain.Entities;
    using Inventra.Domain.Interfaces;
    using MediatR;

    namespace Inventra.Application.Items.Commands.CreateItem
    {
        public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDto>
        {
            private readonly IItemRepository _itemRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ICustomIdGenerator _customIdGenerator;
            private readonly ICurrentUserService _currentUserService;

            public CreateItemCommandHandler(IItemRepository itemRepository,IUnitOfWork unitOfWork, IMapper mapper, 
                ICustomIdGenerator customIdGenerator, ICurrentUserService currentUserService)
            {
                _itemRepository = itemRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _customIdGenerator = customIdGenerator;
                _currentUserService = currentUserService;
            }

            public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
            {
                var userId = _currentUserService.UserId
                    ?? throw new UnauthorizedAccessException("User is not authenticated");

                var customId = await _customIdGenerator.GenerateAsync(request.InventoryId, cancellationToken);

                var item = new Item(request.InventoryId, userId, customId);

                item.UpdateValues(
                    request.CustomString1Value, request.CustomString2Value, request.CustomString3Value,
                    request.CustomInt1Value, request.CustomInt2Value, request.CustomInt3Value,
                    request.CustomText1Value, request.CustomText2Value, request.CustomText3Value,
                    request.CustomBool1Value, request.CustomBool2Value, request.CustomBool3Value,
                    request.CustomLink1Value, request.CustomLink2Value, request.CustomLink3Value,
                    request.ImageUrl);

                await _itemRepository.AddAsync(item, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return _mapper.Map<ItemDto>(item);
            }
        }
    }
