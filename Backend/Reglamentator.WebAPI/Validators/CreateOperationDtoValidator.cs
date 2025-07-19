using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class CreateOperationDtoValidator: AbstractValidator<CreateOperationDto>
{
    public CreateOperationDtoValidator()
    {
        RuleFor(x => x.Theme)
            .NotEmpty()
            .WithMessage("Тема операции обязательна");

        RuleFor(x => x.StartDate)
            .NotNull()
            .WithMessage("Дата начала обязательна");
            
        RuleForEach(x => x.Reminders)
            .SetValidator(new CreateReminderDtoValidator());
    }
}