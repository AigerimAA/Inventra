using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Access.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<AccessUserDto>>
    {
        private readonly IAccessRepository _accessRepository;
        public SearchUsersQueryHandler(IAccessRepository accessRepository)
            => _accessRepository = accessRepository;

        public async Task<IEnumerable<AccessUserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _accessRepository.SearchUsersAsync(request.Query, cancellationToken);
            return users.Select(u => new AccessUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email
            });
        }
    }
}
