using FluentValidation;

namespace BackendBase.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Input validation for <see cref="UpdateProductCommand"/>. Runs automatically
/// via the MediatR <c>ValidationBehavior</c> before the handler executes.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or greater.");
    }
}
