using FluentValidation;
using Google.Protobuf.WellKnownTypes;

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
            .WithMessage("Дата начала обязательна")
            .Must(BeValidDate)
            .WithMessage("Дата начала должна быть в будущем");
            
        RuleForEach(x => x.Reminders)
            .SetValidator(new CreateReminderDtoValidator());
    }
    
    private bool BeValidDate(Timestamp date)
    {
        return date.ToDateTime() >= DateTime.UtcNow;
    }
}