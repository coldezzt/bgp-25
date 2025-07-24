using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class DeleteReminderRequestValidator : AbstractValidator<DeleteReminderRequest>
{
    public DeleteReminderRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
            
        RuleFor(x => x.OperationId)
            .GreaterThan(0)
            .WithMessage("ID операции должен быть положительным числом");
            
        RuleFor(x => x.ReminderId)
            .GreaterThan(0)
            .WithMessage("ID напоминания должен быть положительным числом");
    }
}