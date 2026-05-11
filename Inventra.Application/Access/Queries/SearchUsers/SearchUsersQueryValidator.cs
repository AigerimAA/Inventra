using FluentValidation;

namespace Inventra.Application.Access.Queries.SearchUsers
{
    public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
    {
        public SearchUsersQueryValidator()
        {
            RuleFor(x => x.Query)
                .NotEmpty().WithMessage("Search query is required")
                .MinimumLength(2).WithMessage("Search query must be at least 2 characters");
        }
    }
}
