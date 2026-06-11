using FluentValidation;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests.Validators;

public sealed class CreateRequestDtoValidator : AbstractValidator<CreateRequestDto>
{
    public CreateRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(RequestConsts.MinTitleLength)
            .MaximumLength(RequestConsts.MaxTitleLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(RequestConsts.MaxDescriptionLength);

        RuleFor(x => x.RequestTypeId)
            .NotEmpty();

        RuleFor(x => x.OrganizationUnitId)
            .NotEmpty();

        RuleFor(x => x.Justification)
            .MaximumLength(RequestConsts.MaxJustificationLength);

        RuleFor(x => x.DueDate)
            .GreaterThan(System.DateTime.UtcNow.Date)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future.");
    }
}
