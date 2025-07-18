using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class OperationHistoryRequestValidator : AbstractValidator<OperationHistoryRequest>
{
    public OperationHistoryRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
    }
}