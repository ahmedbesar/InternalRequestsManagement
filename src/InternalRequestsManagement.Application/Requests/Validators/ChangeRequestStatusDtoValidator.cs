using FluentValidation;
using InternalRequestsManagement.Requests;

namespace InternalRequestsManagement.Requests.Validators;

public sealed class ChangeRequestStatusDtoValidator : AbstractValidator<ChangeRequestStatusDto>
{
    public ChangeRequestStatusDtoValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(RequestConsts.MaxStatusNoteLength);
    }
}
