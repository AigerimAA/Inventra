using FluentValidation;

namespace Inventra.Application.Inventories.Commands.UpdateInventory
{
    public class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
    {
        public UpdateInventoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid inventory ID");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required");

            RuleFor(x => x.Version)
                .NotEmpty().WithMessage("Version is required for concurrency check");
        }
    }
}
