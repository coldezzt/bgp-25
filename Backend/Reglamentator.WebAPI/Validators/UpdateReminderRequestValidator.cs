using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class UpdateReminderRequestValidator : AbstractValidator<UpdateReminderRequest>
{
    public UpdateReminderRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
            
        RuleFor(x => x.OperationId)
            .GreaterThan(0)
            .WithMessage("ID операции должен быть положительным числом");
            
        RuleFor(x => x.Reminder)
            .NotNull()
            .WithMessage("Напоминание не может быть null")
            .SetValidator(new UpdateReminderDtoValidator());
    }
}